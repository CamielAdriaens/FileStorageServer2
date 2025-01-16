using Moq;
using Microsoft.AspNetCore.SignalR;
using LOGIC;
using Xunit;
using System.Threading.Tasks;

namespace FileStorage.Tests
{
    public class WebSocketServiceTests
    {
        private readonly Mock<IHubContext<FileSharingHub>> _mockHubContext;
        private readonly Mock<IClientProxy> _mockClientProxy;
        private readonly WebSocketService _webSocketService;

        public WebSocketServiceTests()
        {
            // Mock the IHubContext and IClientProxy
            _mockHubContext = new Mock<IHubContext<FileSharingHub>>();
            _mockClientProxy = new Mock<IClientProxy>();

            // Mock the Clients.User() method to return a mock IClientProxy
            _mockHubContext.Setup(h => h.Clients.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);

            // Initialize the WebSocketService with the mocked IHubContext
            _webSocketService = new WebSocketService(_mockHubContext.Object);
        }

       

        [Fact]
        public async Task SendMessageToUser_ShouldCallClientsUserMethod_WhenCalled()
        {
            // Arrange
            var userEmail = "testuser@example.com";
            var message = "Test Message";

            // Act
            await _webSocketService.SendMessageToUser(userEmail, message);

            // Assert
            // Verify that the Clients.User method was called with the correct user email.
            _mockHubContext.Verify(h => h.Clients.User(userEmail), Times.Once);
        }
    }
}
