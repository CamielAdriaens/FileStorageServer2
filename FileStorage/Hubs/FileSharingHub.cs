using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class FileSharingHub : Hub
{
    public async Task SendMessageToUser(string userEmail, string message)
    {
        await Clients.User(userEmail).SendAsync("ReceiveMessage", message);
    }
}
