using Microsoft.AspNetCore.SignalR;

namespace VoiceService.Services;

public class VoiceHub : Hub
{
    private readonly RoomManager _roomManager;

    public VoiceHub(RoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public async Task JoinRoom(string roomId)
    {
        var connectionId = Context.ConnectionId;

        _roomManager.JoinRoom(roomId, connectionId);

        var users = _roomManager.GetUsersInRoom(roomId);

        Console.WriteLine($"User {connectionId} joined {roomId}");

        if (users.Count == 2)
        {
            await Clients.Client(users[0]).SendAsync("StartCall", true);
            await Clients.Client(users[1]).SendAsync("StartCall", false);
        }
    }

    public async Task SendOffer(object offer)
    {
        var connectionId = Context.ConnectionId;
        var roomId = _roomManager.GetRoom(connectionId);

        if (roomId == null) return;

        var users = _roomManager.GetUsersInRoom(roomId);

        var target = users.FirstOrDefault(u => u != connectionId);
        if (target == null) return;

        await Clients.Client(target).SendAsync("ReceiveOffer", offer);
    }

    public async Task SendAnswer(object answer)
    {
        var connectionId = Context.ConnectionId;
        var roomId = _roomManager.GetRoom(connectionId);

        if (roomId == null) return;

        var users = _roomManager.GetUsersInRoom(roomId);

        var target = users.FirstOrDefault(u => u != connectionId);
        if (target == null) return;

        await Clients.Client(target).SendAsync("ReceiveAnswer", answer);
    }

    public async Task SendIceCandidate(object candidate)
    {
        var connectionId = Context.ConnectionId;
        var roomId = _roomManager.GetRoom(connectionId);

        if (roomId == null) return;

        var users = _roomManager.GetUsersInRoom(roomId);

        var target = users.FirstOrDefault(u => u != connectionId);
        if (target == null) return;

        await Clients.Client(target).SendAsync("ReceiveIceCandidate", candidate);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var roomId = _roomManager.GetRoom(connectionId);

        _roomManager.Remove(connectionId);

        if (roomId != null)
        {
            var users = _roomManager.GetUsersInRoom(roomId);

            foreach (var user in users)
            {
                await Clients.Client(user).SendAsync("UserDisconnected");
            }
        }

        Console.WriteLine($"User {connectionId} disconnected");

        await base.OnDisconnectedAsync(exception);
    }
}