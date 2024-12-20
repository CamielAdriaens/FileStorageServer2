﻿using MODELS;
using DAL;
using System.Collections.Generic;
using System.Threading.Tasks;
using INTERFACES;
using System.Configuration;
using Microsoft.EntityFrameworkCore;


namespace LOGIC
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetOrCreateUserByGoogleIdAsync(string googleId, string email, string name)
        {
            try
            {
                // Check if the user already exists in the database
                var user = await _userRepository.GetUserByGoogleId(googleId);
                if (user == null)
                {
                    Console.WriteLine($"Creating new user with Google ID: {googleId}");

                    // Create a new user and verify properties
                    user = new User
                    {
                        GoogleId = googleId,
                        Email = email ?? "Unknown", // Fallback if email is null
                        Name = name ?? "Unknown"    // Fallback if name is null
                    };

                    user = await _userRepository.CreateUser(user);
                    Console.WriteLine($"User {googleId} created successfully with ID: {user.UserId}.");
                }
                else
                {
                    Console.WriteLine($"User {googleId} already exists.");
                }

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetOrCreateUserByGoogleIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<UserFile>> GetUserFilesAsync(string googleId)
        {
            return await _userRepository.GetUserFiles(googleId);
        }

        public async Task AddUserFileAsync(string googleId, string mongoFileId, string fileName)
        {
            var user = await _userRepository.GetUserByGoogleId(googleId);
            if (user != null)
            {
                var userFile = new UserFile
                {
                    MongoFileId = mongoFileId,
                    FileName = fileName,
                    UploadDate = DateTime.UtcNow,
                    UserId = user.UserId
                };

                await _userRepository.AddUserFile(userFile);
            }
        }

        public async Task RemoveUserFileAsync(string googleId, string mongoFileId)
        {
            var user = await _userRepository.GetUserByGoogleId(googleId);
            if (user != null)
            {
                await _userRepository.RemoveUserFile(user.UserId, mongoFileId);
            }
        }
        public async Task ShareFileAsync(string senderGoogleId, string recipientEmail, string fileName, string mongoFileId)
        {
            // Retrieve the sender and recipient from the database
            var sender = await _userRepository.GetUserByGoogleId(senderGoogleId);
            var recipient = await _userRepository.GetUserByEmail(recipientEmail);

            if (recipient == null)
                throw new Exception("Recipient user not found");

            // Retrieve the file using the MongoFileId
            var userFile = await _userRepository.GetFileByMongoFileId(mongoFileId); // Assuming you have a method to fetch a UserFile by MongoFileId

            if (userFile == null)
                throw new Exception("User file not found");

            // Create a PendingFileShare record
            var shareRequest = new PendingFileShare
            {
                FileName = fileName,
                MongoFileId = mongoFileId,
                FileId = userFile.FileId, // Set the FileId from the retrieved UserFile
                SenderUserId = sender.UserId,
                RecipientUserId = recipient.UserId,
                CreatedAt = DateTime.UtcNow, // Set the current time for when the file is shared
                IsAccepted = false // Default value for IsAccepted can be set to false initially
            };

            // Add the PendingFileShare record to the repository
            await _userRepository.AddFileShare(shareRequest);
        }


        public async Task AcceptFileShareAsync(int shareId)
        {
            var shareRequest = await _userRepository.GetFileShareById(shareId);

            if (shareRequest == null)
                throw new Exception("Share request not found");

            await _userRepository.AcceptFileShare(shareRequest);
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
            var user = await _userRepository.GetUserByGoogleId(googleId);
            if (user == null)
                throw new Exception("User not found");

            // Retrieve pending file share requests for the user
            var pendingShares = await _userRepository.GetPendingFileSharesForUserAsync(user.UserId);
            return pendingShares;
        }

    }
}