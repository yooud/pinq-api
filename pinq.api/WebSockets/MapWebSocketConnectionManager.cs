using System.Collections.Concurrent;

namespace pinq.api.WebSockets;

public class MapWebSocketConnectionManager : IWebSocketConnectionManager
{
    private readonly ConcurrentDictionary<int, WebSocketHandler> _connections = new();
    private readonly ConcurrentBag<WebSocketHandler> _unauthenticatedConnections = new();

    public void AddConnection(int userId, WebSocketHandler socket)
    {
        if (_unauthenticatedConnections.Contains(socket)) 
            _unauthenticatedConnections.TryTake(out socket);
        _connections[userId] = socket;
    }

    public void AddUnauthenticatedConnection(WebSocketHandler socket) => _unauthenticatedConnections.Add(socket);

    public WebSocketHandler? GetConnection(int userId)
    {
        _connections.TryGetValue(userId, out var socket);
        return socket;
    }

    public IEnumerable<WebSocketHandler> GetUnauthenticatedConnections() => _unauthenticatedConnections.ToArray();

    public void RemoveConnection(int userId) => _connections.TryRemove(userId, out _);

    public void RemoveUnauthenticatedConnection(WebSocketHandler socket) => _unauthenticatedConnections.TryTake(out socket);

    public IEnumerable<KeyValuePair<int, WebSocketHandler>> GetConnections() => _connections.ToArray();

    public async Task CloseAllConnectionsAsync(string reason)
    {
        var authenticatedConnections = GetConnections();
        foreach (var handler in authenticatedConnections) 
            await handler.Value.CloseConnectionAsync(reason);

        var unauthenticatedConnections = GetUnauthenticatedConnections();
        foreach (var handler in unauthenticatedConnections) 
            await handler.CloseConnectionAsync(reason);
    }
}
