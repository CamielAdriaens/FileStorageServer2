using MODELS;
using Microsoft.EntityFrameworkCore;
using INTERFACES; // For repository interfaces

using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<User> GetUserByUserId(int userId)
        {
            return await _context.Users
                .Include(u => u.UserFiles) // Include related UserFiles if needed
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User> GetUserByGoogleId(string googleId)
        {
            return await _context.Users
                .Include(u => u.UserFiles)
                .FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }
        public async Task<UserFile> GetFileByMongoFileId(string mongoFileId)
        {
            return await _context.UserFiles
                .FirstOrDefaultAsync(file => file.MongoFileId == mongoFileId);
        }

        public async Task<User> CreateUser(User user)
        {
            try
            {
                Console.WriteLine("Attempting to add user to database...");
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                Console.WriteLine($"User with Google ID {user.GoogleId} saved to database with ID {user.UserId}.");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving user to database: {ex.Message}");
                throw;
            }
        }

        public async Task AddUserFile(UserFile file)
        {
            _context.UserFiles.Add(file);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserFile>> GetUserFiles(string googleId)
        {
            var user = await _context.Users
                .Include(u => u.UserFiles)
                .FirstOrDefaultAsync(u => u.GoogleId == googleId);

            return user?.UserFiles ?? new List<UserFile>();
        }

        public async Task RemoveUserFile(int userId, string mongoFileId)
        {
            var userFile = await _context.UserFiles
                .FirstOrDefaultAsync(f => f.UserId == userId && f.MongoFileId == mongoFileId);

            if (userFile != null)
            {
                _context.UserFiles.Remove(userFile);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddFileShare(PendingFileShare shareRequest)
        {
            _context.PendingFileShares.Add(shareRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<PendingFileShare> GetFileShareById(int shareId)
        {
            return await _context.PendingFileShares
                .Include(s => s.File)
                .FirstOrDefaultAsync(s => s.ShareId == shareId);
        }

        public async Task AcceptFileShare(PendingFileShare shareRequest)
        {
            shareRequest.IsAccepted = true;
            await _context.SaveChangesAsync();
        }
        public async Task RemoveFileShareAsync(PendingFileShare share)
        {
            _context.PendingFileShares.Remove(share);
            await _context.SaveChangesAsync();
        }
        public async Task<List<PendingFileShare>> GetPendingFileSharesForUserAsync(int userId)
        {
            return await _context.PendingFileShares
                .Where(s => s.RecipientUserId == userId && s.IsAccepted == false)
                .ToListAsync();
        }
        public async Task<PendingFileShare> GetFileShareById(string fileId)
        {
            return await _context.PendingFileShares
                                 .Where(share => share.MongoFileId == fileId)
                                 .FirstOrDefaultAsync();
        }
        public async Task UpdateFileShare(PendingFileShare shareRequest)
        {
            _context.PendingFileShares.Update(shareRequest);
            await _context.SaveChangesAsync();
        }




    }
}