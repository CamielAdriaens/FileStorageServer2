using System.Collections.Generic;
using System.Threading.Tasks;
using MODELS;
namespace INTERFACES
{
    public interface IUserService
    {
        Task<User> GetOrCreateUserByGoogleIdAsync(string googleId, string email, string name);
        Task<List<UserFile>> GetUserFilesAsync(string googleId);
        Task AddUserFileAsync(string googleId, string mongoFileId, string fileName);
        Task RemoveUserFileAsync(string googleId, string mongoFileId); // Add this method
        Task ShareFileAsync(string senderGoogleId, string recipientEmail, string fileName, string mongoFileId);
        Task AcceptFileShareAsync(int shareId);
        Task<List<PendingFileShare>> GetPendingSharesAsync(string googleId);
        Task RefuseFileShareAsync(int shareId);

    }
}
