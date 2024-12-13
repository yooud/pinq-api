using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FirebaseAdmin.Auth;
using Microsoft.IdentityModel.Tokens;
using pinq.api.Services;

namespace pinq.api.WebSockets;

public abstract class WebSocketHandler(IAuthorizationService authorizationService, ISessionCacheService sessionService)
{
    private WebSocket _webSocket;
    protected bool IsAuthorized = false;
    protected FirebaseToken Token;
    
    private async Task<bool> ValidateAuthorizationAsync(JsonElement message)
    {
        var token = message.GetProperty("data").GetProperty("token").GetString();
        if (token.IsNullOrEmpty())
            return false;
        var isAuthorized = await authorizationService.ValidateTokenAsync(token);
        if (isAuthorized) 
            return true;
        
        Token = await authorizationService.GetTokenAsync(token);
        var session = message.GetProperty("data").GetProperty("session").GetString();
        var isValid = await sessionService.ValidateSessionAsync(Token.Uid, session);
        return isValid;
    }

    // protected async Task<bool> ValidateSessionAsync(string sessionId) => await sessionValidationService.ValidateSessionAsync(sessionId);

    public async Task HandleAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        _webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await CommunicateWithClientAsync();
    }
    
    protected async Task SendMessage(object message)
    {
        var messageJson = JsonSerializer.Serialize(message);
        var messageBuffer = Encoding.UTF8.GetBytes(messageJson);
        await _webSocket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task CommunicateWithClientAsync()
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result;

        do
        {
            result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType != WebSocketMessageType.Text) continue;

            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            
            JsonDocument jsonDoc;
            try
            {
                jsonDoc = JsonDocument.Parse(receivedMessage);

                if (IsAuthorized)
                {
                    await HandleMessageAsync(jsonDoc.RootElement);
                }
                else
                {
                    if (jsonDoc.RootElement.GetProperty("type").GetString().Equals("auth"))
                    {
                        IsAuthorized = await ValidateAuthorizationAsync(jsonDoc.RootElement);
                        if (IsAuthorized) continue;
                    }

                    var response = new { type = "error", message = "Unauthorized" };
                    await SendMessage(response);
                }
            }
            catch (Exception ex)
            {
                var response = new { type = "error", message = "Invalid JSON format" };
                await SendMessage(response);
            }
        } while (!result.CloseStatus.HasValue);

        await _webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
 
    protected abstract Task HandleMessageAsync(JsonElement message);
}