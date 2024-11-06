using Models;
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
    }
}
