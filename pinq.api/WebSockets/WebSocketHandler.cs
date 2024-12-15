using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FirebaseAdmin.Auth;
using pinq.api.Repository;
using pinq.api.Services;

namespace pinq.api.WebSockets;

public abstract class WebSocketHandler(
    IAuthorizationService authorizationService,
    ISessionCacheService sessionService,
    IUserProfileRepository profileRepository)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private WebSocket? _webSocket;
    private bool _isAuthorized;
    
    protected FirebaseToken Token;
    protected IWebSocketConnectionManager ConnectionManager;
    public int UserId { get; private set; }
    public string Username { get; private set; }   

    public async Task HandleAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        _webSocket = await context.WebSockets.AcceptWebSocketAsync();
        ConnectionManager = context.RequestServices.GetRequiredService<MapWebSocketConnectionManager>();
        ConnectionManager.AddUnauthenticatedConnection(this);
        await CommunicateWithClientAsync();
    }
    
    public async Task SendMessageAsync(object message)
    {
        if (_webSocket?.State != WebSocketState.Open) return;

        var messageJson = JsonSerializer.Serialize(message, JsonOptions);
        var messageBuffer = Encoding.UTF8.GetBytes(messageJson);
        await _webSocket.SendAsync(
            new ArraySegment<byte>(messageBuffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    public async Task CloseConnectionAsync(string reason = "Server shutting down")
    {
        if (_webSocket?.State is WebSocketState.Open or WebSocketState.CloseReceived or WebSocketState.CloseSent) 
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);

        ConnectionManager.RemoveConnection(UserId);
        ConnectionManager.RemoveUnauthenticatedConnection(this);
    }

    private async Task<bool> ValidateAuthorizationAsync(JsonElement message)
    {
        if (!message.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("token", out var tokenProperty) ||
            string.IsNullOrEmpty(tokenProperty.GetString()))
            return false;

        var token = tokenProperty.GetString();
        if (!await authorizationService.ValidateTokenAsync(token)) return false;

        Token = await authorizationService.GetTokenAsync(token);

        if (!data.TryGetProperty("session", out var sessionProperty) || string.IsNullOrEmpty(sessionProperty.GetString()))
            return false;

        return await sessionService.ValidateSessionAsync(Token.Uid, sessionProperty.GetString());
    }

    private async Task CommunicateWithClientAsync()
    {
        var buffer = new byte[4 * 1024];

        try
        {
            while (_webSocket?.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close) break;

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    try
                    {
                        var jsonDoc = JsonDocument.Parse(receivedMessage);
                        await HandleReceivedMessageAsync(jsonDoc.RootElement);
                    }
                    catch (JsonException)
                    {
                        await SendMessageAsync(new { type = "error", message = "Invalid JSON format" });
                    }
                }
            }
        }
        finally
        {
            await CloseConnectionAsync();
        }
    }

    private async Task HandleReceivedMessageAsync(JsonElement message)
    {
        if (_isAuthorized)
        {
            await HandleMessageAsync(message);
        }
        else if (message.TryGetProperty("type", out var typeProperty) &&
                 typeProperty.GetString()?.Equals("auth", StringComparison.OrdinalIgnoreCase) == true)
        {
            _isAuthorized = await ValidateAuthorizationAsync(message);

            if (_isAuthorized && Token != null)
            {
                var profile = await profileRepository.GetProfileByUid(Token.Uid);
                if (profile is null)
                {
                    _isAuthorized = false;
                    await SendMessageAsync(new { type = "error", message = "The profile is not complete" });
                    return;
                }

                UserId = profile.UserId;
                Username = profile.Username;
                ConnectionManager.AddConnection(UserId, this);
                await OnInitialAsync();
            }
            else
            {
                await SendMessageAsync(new { type = "error", message = "Unauthorized" });
            }
        }
        else
        {
            await SendMessageAsync(new { type = "error", message = "Unauthorized" });
        }
    }

    protected abstract Task HandleMessageAsync(JsonElement message);

    protected abstract Task OnInitialAsync();
}
