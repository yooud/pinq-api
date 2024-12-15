namespace pinq.api.WebSockets;

public interface IWebSocketConnectionManager
{
    public void AddConnection(int userId, WebSocketHandler socket);

    public void AddUnauthenticatedConnection(WebSocketHandler socket);

    public WebSocketHandler? GetConnection(int userId);

    public void RemoveConnection(int userId);
    
    public void RemoveUnauthenticatedConnection(WebSocketHandler socket);

    public IEnumerable<KeyValuePair<int, WebSocketHandler>> GetConnections();
    
    public IEnumerable<WebSocketHandler> GetUnauthenticatedConnections();

    public Task CloseAllConnectionsAsync(string reason);
}