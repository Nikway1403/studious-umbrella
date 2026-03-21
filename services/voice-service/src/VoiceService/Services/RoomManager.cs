namespace VoiceService.Services;

public class RoomManager
{
    private readonly Dictionary<string, List<string>> _rooms = new();
    private readonly Dictionary<string, string> _connections = new();

    public void JoinRoom(string roomId, string connectionId)
    {
        if (!_rooms.ContainsKey(roomId))
            _rooms[roomId] = new List<string>();

        _rooms[roomId].Add(connectionId);
        _connections[connectionId] = roomId;
    }

    public List<string> GetUsersInRoom(string roomId)
    {
        return _rooms.TryGetValue(roomId, out var users)
            ? users
            : new List<string>();
    }

    public string? GetRoom(string connectionId)
    {
        return _connections.TryGetValue(connectionId, out var room)
            ? room
            : null;
    }

    public void Remove(string connectionId)
    {
        if (!_connections.TryGetValue(connectionId, out var roomId))
            return;

        _connections.Remove(connectionId);

        if (_rooms.TryGetValue(roomId, out var users))
        {
            users.Remove(connectionId);

            if (users.Count == 0)
                _rooms.Remove(roomId);
        }
    }
}