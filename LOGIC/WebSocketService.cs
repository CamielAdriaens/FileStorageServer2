using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using INTERFACES;

namespace LOGIC
{
    public class WebSocketService : IWebSocketService
    {
        private readonly IHubContext<FileSharingHub> _hubContext;

        public WebSocketService(IHubContext<FileSharingHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendMessageToUser(string userEmail, string message)
        {
            await _hubContext.Clients.User(userEmail).SendAsync("ReceiveMessage", message);
        }
    }
}
