using System.Collections.Concurrent;

namespace pinq.api.WebSockets;

public class MapWebSocketConnectionManager : IWebSocketConnectionManager
{
    private readonly ConcurrentDictionary<int, WebSocketHandler> _connections = new();

    public void AddConnection(int userId, WebSocketHandler socket) => _connections[userId] = socket;

    public WebSocketHandler? GetConnection(int userId)
    {
        _connections.TryGetValue(userId, out var socket);
        return socket;
    }

    public void RemoveConnection(int userId) => _connections.TryRemove(userId, out _);

    public IEnumerable<KeyValuePair<int, WebSocketHandler>> GetAllConnections() => _connections.ToArray();
}
