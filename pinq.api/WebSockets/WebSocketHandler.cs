using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FirebaseAdmin.Auth;
using pinq.api.Repository;
using pinq.api.Services;

namespace pinq.api.WebSockets;

public abstract class WebSocketHandler(
    IAuthorizationService authorizationService,
    ISessionCacheService sessionService,
    IUserRepository userRepository)
{
    private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
    private WebSocket _webSocket;
    private bool _isAuthorized;
    
    protected FirebaseToken Token;
    protected IWebSocketConnectionManager ConnectionManager;
    
    public int UserId { get; set; }
    
    private async Task<bool> ValidateAuthorizationAsync(JsonElement message)
    {
        var token = message.GetProperty("data").GetProperty("token").GetString();
        if (string.IsNullOrEmpty(token))
            return false;

        var isAuthorized = await authorizationService.ValidateTokenAsync(token);
        if (!isAuthorized)
            return false;

        Token = await authorizationService.GetTokenAsync(token);
        var session = message.GetProperty("data").GetProperty("session").GetString();
        var isValid = await sessionService.ValidateSessionAsync(Token.Uid, session);
        return isValid;
    }

    public async Task HandleAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        ConnectionManager = context.RequestServices.GetRequiredService<MapWebSocketConnectionManager>();
        _webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await CommunicateWithClientAsync();
    }
    
    public async Task SendMessage(object message)
    {
        var messageJson = JsonSerializer.Serialize(message, _jsonOptions);
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

                if (_isAuthorized)
                {
                    await HandleMessageAsync(jsonDoc.RootElement);
                }
                else
                {
                    if (jsonDoc.RootElement.GetProperty("type").GetString().Equals("auth"))
                    {
                        _isAuthorized = await ValidateAuthorizationAsync(jsonDoc.RootElement);
                        if (_isAuthorized)
                        {
                            var user = await userRepository.GetUserByUid(Token.Uid);
                            UserId = user.Id;
                            ConnectionManager.AddConnection(UserId, this);
                            
                            await OnInitialAsync();
                            continue;
                        }
                    }

                    var response = new { type = "error", message = "Unauthorized" };
                    await SendMessage(response);
                }
            }
            catch (JsonException ex)
            {
                var response = new { type = "error", message = "Invalid JSON format" };
                await SendMessage(response);
            }
            catch (KeyNotFoundException ex)
            {
                var response = new { type = "error", message = "Invalid JSON format" };
                await SendMessage(response);
            }
        } while (!result.CloseStatus.HasValue);
        
        ConnectionManager.RemoveConnection(UserId);
        await _webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
 
    protected abstract Task HandleMessageAsync(JsonElement message);

    protected abstract Task OnInitialAsync();
}
