using Microsoft.AspNetCore.Mvc;
using INTERFACES;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IWebSocketService _webSocketService;

        public MessageController(IWebSocketService webSocketService)
        {
            _webSocketService = webSocketService;
        }

        // POST api/message/send
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrEmpty(request.UserEmail) || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("User email and message are required.");
            }

            await _webSocketService.SendMessageToUser(request.UserEmail, request.Message);
            return Ok("Message sent successfully.");
        }
    }

    public class SendMessageRequest
    {
        public string UserEmail { get; set; }
        public string Message { get; set; }
    }
}
