namespace pinq.api.WebSockets;

public interface IWebSocketConnectionManager
{
    public void AddConnection(int userId, WebSocketHandler socket);

    public WebSocketHandler? GetConnection(int userId);

    public void RemoveConnection(int userId);

    public IEnumerable<KeyValuePair<int, WebSocketHandler>> GetAllConnections();
}