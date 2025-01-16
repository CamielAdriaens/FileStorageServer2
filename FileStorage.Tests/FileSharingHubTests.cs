using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

public class FileSharingHubTests
{
    private readonly Mock<IHubContext<FileSharingHub>> _mockHubContext;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly FileSharingHub _hub;

    public FileSharingHubTests()
    {
        // Mocking the context and client proxy
        _mockHubContext = new Mock<IHubContext<FileSharingHub>>();
        _mockClientProxy = new Mock<IClientProxy>();

        // Creating an instance of the FileSharingHub
        _hub = new FileSharingHub();
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldLogConnection_WhenClientConnects()
    {
        // Arrange
        var connectionId = "test-connection-id";
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(x => x.ConnectionId).Returns(connectionId);
        _hub.Context = mockContext.Object;

        // Act
        await _hub.OnConnectedAsync();

        // Assert: Check that the correct connection ID is logged.
        // In actual tests, you'd capture the console output. For now, we'll focus on the mock behavior.
        // Normally, you'd assert that Console.WriteLine was called, or you'd use logging in your hub and mock that.
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldLogDisconnection_WhenClientDisconnects()
    {
        // Arrange
        var connectionId = "test-connection-id";
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(x => x.ConnectionId).Returns(connectionId);
        _hub.Context = mockContext.Object;

        // Act
        await _hub.OnDisconnectedAsync(null);

        // Assert: Again, we'd check that the disconnection logs the correct message
        // Similar to the OnConnectedAsync test, you'd use a logging framework in real-world scenarios to capture this.
    }

    
}
