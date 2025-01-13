using MODELS;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace INTERFACES
{
    public interface IUserRepository
    {
        Task<User> GetUserByGoogleId(string googleId);
        Task<User> CreateUser(User user);
        Task AddUserFile(UserFile file);
        Task<List<UserFile>> GetUserFiles(string googleId);
        Task RemoveUserFile(int userId, string mongoFileId); // Add this method
        Task<User> GetUserByEmail(string email);
        Task AddFileShare(PendingFileShare shareRequest);
        Task<PendingFileShare> GetFileShareById(int shareId);
        Task AcceptFileShare(PendingFileShare shareRequest);
        Task RemoveFileShareAsync(PendingFileShare share);
        Task<List<PendingFileShare>> GetPendingFileSharesForUserAsync(int userId);
        Task<UserFile> GetFileByMongoFileId(string mongoFileId);
        Task<PendingFileShare> GetFileShareById(string fileId);
        Task UpdateFileShare(PendingFileShare shareRequest);
        Task<User> GetUserByUserId(int userId);


    }
}
