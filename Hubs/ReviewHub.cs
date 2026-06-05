using Microsoft.AspNetCore.SignalR;

namespace CineScope.Hubs;

public class ReviewHub : Hub
{
    private static int _activeConnections;

    public static int CurrentActiveUsers => Math.Max(0, Volatile.Read(ref _activeConnections));

    public override async Task OnConnectedAsync()
    {
        var activeConnections = Interlocked.Increment(ref _activeConnections);
        await Clients.Caller.SendAsync("ConnectionIdReceived", Context.ConnectionId);
        await Clients.All.SendAsync("ActiveUsersUpdated", activeConnections);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var activeConnections = Interlocked.Decrement(ref _activeConnections);

        if (activeConnections < 0)
        {
            Interlocked.Exchange(ref _activeConnections, 0);
            activeConnections = 0;
        }

        await Clients.All.SendAsync("ActiveUsersUpdated", activeConnections);

        await base.OnDisconnectedAsync(exception);
    }
}
