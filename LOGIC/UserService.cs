using MODELS;
using DAL;
using System.Collections.Generic;
using System.Threading.Tasks;
using INTERFACES;
using System;

namespace LOGIC
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<User> GetOrCreateUserByGoogleIdAsync(string googleId, string email, string name)
        {
            try
            {
                var user = await _userRepository.GetUserByGoogleId(googleId);
                if (user != null)
                {
                    Console.WriteLine($"User {googleId} already exists.");
                    return user;
                }

                return await CreateUserAsync(googleId, email, name);
            }
            catch (Exception ex)
            {
                LogError(nameof(GetOrCreateUserByGoogleIdAsync), ex);
                throw;
            }
        }

        private async Task<User> CreateUserAsync(string googleId, string email, string name)
        {
            var newUser = new User
            {
                GoogleId = googleId,
                Email = email ?? "Unknown",
                Name = name ?? "Unknown"
            };

            var createdUser = await _userRepository.CreateUser(newUser);
            Console.WriteLine($"User {googleId} created successfully with ID: {createdUser.UserId}.");
            return createdUser;
        }

        public async Task<List<UserFile>> GetUserFilesAsync(string googleId)
        {
            return await _userRepository.GetUserFiles(googleId);
        }

        public async Task AddUserFileAsync(string googleId, string mongoFileId, string fileName)
        {
            var user = await GetUserByGoogleIdOrThrowAsync(googleId);
            var userFile = new UserFile
            {
                MongoFileId = mongoFileId,
                FileName = fileName,
                UploadDate = DateTime.UtcNow,
                UserId = user.UserId
            };

            await _userRepository.AddUserFile(userFile);
        }

        public async Task RemoveUserFileAsync(string googleId, string mongoFileId)
        {
            var user = await GetUserByGoogleIdOrThrowAsync(googleId);
            await _userRepository.RemoveUserFile(user.UserId, mongoFileId);
        }

        public async Task ShareFileAsync(string senderGoogleId, string recipientEmail, string fileName, string mongoFileId)
        {
            var sender = await GetUserByGoogleIdOrThrowAsync(senderGoogleId);
            var recipient = await _userRepository.GetUserByEmail(recipientEmail)
                ?? throw new Exception("Recipient user not found");

            var userFile = await _userRepository.GetFileByMongoFileId(mongoFileId)
                ?? throw new Exception("User file not found");

            var shareRequest = new PendingFileShare
            {
                FileName = fileName,
                MongoFileId = mongoFileId,
                FileId = userFile.FileId,
                SenderUserId = sender.UserId,
                RecipientUserId = recipient.UserId,
                CreatedAt = DateTime.UtcNow,
                IsAccepted = false
            };

            await _userRepository.AddFileShare(shareRequest);
        }

        public async Task AcceptFileShareAsync(int shareId)
        {
            var shareRequest = await GetShareRequestOrThrowAsync(shareId);
            if (shareRequest.IsAccepted)
                throw new Exception("File share already accepted.");

            var recipientUser = await _userRepository.GetUserByUserId(shareRequest.RecipientUserId)
                ?? throw new Exception("Recipient user not found.");

            var sharedFile = await _userRepository.GetFileByMongoFileId(shareRequest.MongoFileId)
                ?? throw new Exception("Shared file not found.");

            var userFile = new UserFile
            {
                MongoFileId = sharedFile.MongoFileId,
                FileName = sharedFile.FileName,
                UploadDate = DateTime.UtcNow,
                UserId = recipientUser.UserId
            };

            await _userRepository.AddUserFile(userFile);

            shareRequest.IsAccepted = true;
            await _userRepository.UpdateFileShare(shareRequest);
        }

        public async Task RefuseFileShareAsync(int shareId)
        {
            var shareRequest = await _userRepository.GetFileShareById(shareId);
            if (shareRequest != null)
            {
                await _userRepository.RemoveFileShareAsync(shareRequest);
            }
        }

        public async Task<List<PendingFileShare>> GetPendingSharesAsync(string googleId)
        {
            var user = await GetUserByGoogleIdOrThrowAsync(googleId);
            return await _userRepository.GetPendingFileSharesForUserAsync(user.UserId);
        }

        public async Task AddFileToUserAsync(string googleId, string fileId)
        {
            var user = await GetUserByGoogleIdOrThrowAsync(googleId);
            var shareRequest = await GetAcceptedShareRequestOrThrowAsync(fileId);

            var userFile = new UserFile
            {
                MongoFileId = shareRequest.MongoFileId,
                FileName = shareRequest.FileName,
                UploadDate = DateTime.UtcNow,
                UserId = user.UserId
            };

            await _userRepository.AddUserFile(userFile);
        }

        public async Task<UserFile> GetFileByIdAsync(string fileId)
        {
            var userFile = await _userRepository.GetFileByMongoFileId(fileId);
            return userFile ?? throw new Exception("File not found.");
        }

        private async Task<User> GetUserByGoogleIdOrThrowAsync(string googleId)
        {
            var user = await _userRepository.GetUserByGoogleId(googleId);
            if (user == null)
                throw new Exception("User not found.");
            return user;
        }

        private async Task<PendingFileShare> GetShareRequestOrThrowAsync(int shareId)
        {
            var shareRequest = await _userRepository.GetFileShareById(shareId);
            if (shareRequest == null)
                throw new Exception("Share request not found.");
            return shareRequest;
        }

        private async Task<PendingFileShare> GetAcceptedShareRequestOrThrowAsync(string fileId)
        {
            var shareRequest = await _userRepository.GetFileShareById(fileId);
            if (shareRequest == null || !shareRequest.IsAccepted)
                throw new Exception("The file share has not been accepted yet.");
            return shareRequest;
        }

        private void LogError(string methodName, Exception ex)
        {
            Console.WriteLine($"Error in {methodName}: {ex.Message}");
        }
    }
}
