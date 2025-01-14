namespace INTERFACES
{
    public interface IWebSocketService
    {
        Task SendMessageToUser(string userEmail, string message);
    }
}
