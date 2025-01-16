using MODELS;
using Microsoft.EntityFrameworkCore;
using INTERFACES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User> GetUserByUserId(int userId) =>
            await _context.Users
                .Include(u => u.UserFiles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

        public async Task<User> GetUserByGoogleId(string googleId) =>
            await _context.Users
                .Include(u => u.UserFiles)
                .FirstOrDefaultAsync(u => u.GoogleId == googleId);

        public async Task<User> GetUserByEmail(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<UserFile> GetFileByMongoFileId(string mongoFileId) =>
            await _context.UserFiles.FirstOrDefaultAsync(f => f.MongoFileId == mongoFileId);

        public async Task<User> CreateUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task AddUserFile(UserFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

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
        public async Task AcceptFileShare(PendingFileShare shareRequest)
        {
            shareRequest.IsAccepted = true;
            await _context.SaveChangesAsync();
        }
        public async Task AddFileShare(PendingFileShare shareRequest)
        {
            if (shareRequest == null)
                throw new ArgumentNullException(nameof(shareRequest));

            _context.PendingFileShares.Add(shareRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<PendingFileShare> GetFileShareById(int shareId) =>
            await _context.PendingFileShares
                .Include(s => s.File)
                .FirstOrDefaultAsync(s => s.ShareId == shareId);

        public async Task<PendingFileShare> GetFileShareById(string fileId) =>
            await _context.PendingFileShares
                .FirstOrDefaultAsync(s => s.MongoFileId == fileId);

        public async Task<List<PendingFileShare>> GetPendingFileSharesForUserAsync(int userId) =>
            await _context.PendingFileShares
                .Where(s => s.RecipientUserId == userId && !s.IsAccepted)
                .ToListAsync();

        public async Task UpdateFileShare(PendingFileShare shareRequest)
        {
            if (shareRequest == null)
                throw new ArgumentNullException(nameof(shareRequest));

            _context.PendingFileShares.Update(shareRequest);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFileShareAsync(PendingFileShare share)
        {
            if (share == null)
                throw new ArgumentNullException(nameof(share));

            _context.PendingFileShares.Remove(share);
            await _context.SaveChangesAsync();
        }
    }
}
