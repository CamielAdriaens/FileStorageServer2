using Microsoft.AspNetCore.SignalR;

public class FileSharingHub : Hub
{
    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }

    // Method to send a message to a specific user
    public async Task SendMessageToUser(string userEmail, string message)
    {
        await Clients.User(userEmail).SendAsync("ReceiveMessage", message);
    }
}
