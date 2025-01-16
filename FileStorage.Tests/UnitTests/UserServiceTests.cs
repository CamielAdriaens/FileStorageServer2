using Moq;
using Xunit;
using LOGIC;
using INTERFACES;
using MODELS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileStorage.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _userService = new UserService(_mockUserRepository.Object);
        }

        [Fact]
        public async Task GetOrCreateUserByGoogleIdAsync_ShouldReturnExistingUser_WhenUserExists()
        {
            // Arrange
            var googleId = "googleId123";
            var email = "user@example.com";
            var name = "John Doe";
            var existingUser = new User { GoogleId = googleId, Email = email, Name = name };
            _mockUserRepository.Setup(r => r.GetUserByGoogleId(googleId)).ReturnsAsync(existingUser);

            // Act
            var result = await _userService.GetOrCreateUserByGoogleIdAsync(googleId, email, name);

            // Assert
            Assert.Equal(existingUser, result);
        }

        [Fact]
        public async Task GetOrCreateUserByGoogleIdAsync_ShouldCreateNewUser_WhenUserDoesNotExist()
        {
            // Arrange
            var googleId = "googleId123";
            var email = "user@example.com";
            var name = "John Doe";
            _mockUserRepository.Setup(r => r.GetUserByGoogleId(googleId)).ReturnsAsync((User)null);
            _mockUserRepository.Setup(r => r.CreateUser(It.IsAny<User>())).ReturnsAsync(new User { GoogleId = googleId, Email = email, Name = name });

            // Act
            var result = await _userService.GetOrCreateUserByGoogleIdAsync(googleId, email, name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(googleId, result.GoogleId);
            Assert.Equal(email, result.Email);
            Assert.Equal(name, result.Name);
        }

        [Fact]
        public async Task GetUserFilesAsync_ShouldReturnUserFiles()
        {
            // Arrange
            var googleId = "googleId123";
            var files = new List<UserFile> { new UserFile { FileName = "file1.txt" }, new UserFile { FileName = "file2.txt" } };
            _mockUserRepository.Setup(r => r.GetUserFiles(googleId)).ReturnsAsync(files);

            // Act
            var result = await _userService.GetUserFilesAsync(googleId);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddUserFileAsync_ShouldAddFileToUser()
        {
            // Arrange
            var googleId = "googleId123";
            var mongoFileId = "mongoFileId123";
            var fileName = "file1.txt";
            var user = new User { UserId = 1 };
            _mockUserRepository.Setup(r => r.GetUserByGoogleId(googleId)).ReturnsAsync(user);

            // Act
            await _userService.AddUserFileAsync(googleId, mongoFileId, fileName);

            // Assert
            _mockUserRepository.Verify(r => r.AddUserFile(It.Is<UserFile>(uf => uf.MongoFileId == mongoFileId && uf.FileName == fileName)), Times.Once);
        }

        [Fact]
        public async Task RemoveUserFileAsync_ShouldRemoveFileFromUser()
        {
            // Arrange
            var googleId = "googleId123";
            var mongoFileId = "mongoFileId123";
            var user = new User { UserId = 1 };
            _mockUserRepository.Setup(r => r.GetUserByGoogleId(googleId)).ReturnsAsync(user);

            // Act
            await _userService.RemoveUserFileAsync(googleId, mongoFileId);

            // Assert
            _mockUserRepository.Verify(r => r.RemoveUserFile(user.UserId, mongoFileId), Times.Once);
        }

        [Fact]
        public async Task ShareFileAsync_ShouldCreateShareRequest_WhenShareIsValid()
        {
            // Arrange
            var senderGoogleId = "senderGoogleId";
            var recipientEmail = "recipient@example.com";
            var fileName = "file1.txt";
            var mongoFileId = "mongoFileId123";
            var sender = new User { UserId = 1 };
            var recipient = new User { UserId = 2 };
            var userFile = new UserFile { MongoFileId = mongoFileId, FileName = fileName };

            _mockUserRepository.Setup(r => r.GetUserByGoogleId(senderGoogleId)).ReturnsAsync(sender);
            _mockUserRepository.Setup(r => r.GetUserByEmail(recipientEmail)).ReturnsAsync(recipient);
            _mockUserRepository.Setup(r => r.GetFileByMongoFileId(mongoFileId)).ReturnsAsync(userFile);

            // Act
            await _userService.ShareFileAsync(senderGoogleId, recipientEmail, fileName, mongoFileId);

            // Assert
            _mockUserRepository.Verify(r => r.AddFileShare(It.Is<PendingFileShare>(s => s.FileName == fileName && s.MongoFileId == mongoFileId)), Times.Once);
        }

        [Fact]
        public async Task AcceptFileShareAsync_ShouldAddFileToRecipient_WhenShareIsAccepted()
        {
            // Arrange
            var shareId = 1;
            var shareRequest = new PendingFileShare { IsAccepted = false, FileName = "file1.txt", MongoFileId = "mongoFileId123", SenderUserId = 1, RecipientUserId = 2 };
            var recipient = new User { UserId = 2 };
            var sharedFile = new UserFile { MongoFileId = "mongoFileId123", FileName = "file1.txt" };

            _mockUserRepository.Setup(r => r.GetFileShareById(shareId)).ReturnsAsync(shareRequest);
            _mockUserRepository.Setup(r => r.GetUserByUserId(2)).ReturnsAsync(recipient);
            _mockUserRepository.Setup(r => r.GetFileByMongoFileId("mongoFileId123")).ReturnsAsync(sharedFile);

            // Act
            await _userService.AcceptFileShareAsync(shareId);

            // Assert
            _mockUserRepository.Verify(r => r.AddUserFile(It.Is<UserFile>(uf => uf.MongoFileId == "mongoFileId123" && uf.FileName == "file1.txt")), Times.Once);
            _mockUserRepository.Verify(r => r.UpdateFileShare(It.Is<PendingFileShare>(s => s.IsAccepted == true)), Times.Once);
        }

        [Fact]
        public async Task RefuseFileShareAsync_ShouldRemoveShareRequest_WhenRefused()
        {
            // Arrange
            var shareId = 1;
            var shareRequest = new PendingFileShare();
            _mockUserRepository.Setup(r => r.GetFileShareById(shareId)).ReturnsAsync(shareRequest);

            // Act
            await _userService.RefuseFileShareAsync(shareId);

            // Assert
            _mockUserRepository.Verify(r => r.RemoveFileShareAsync(shareRequest), Times.Once);
        }

        [Fact]
        public async Task GetPendingSharesAsync_ShouldReturnPendingShares()
        {
            // Arrange
            var googleId = "googleId123";
            var pendingShares = new List<PendingFileShare> { new PendingFileShare(), new PendingFileShare() };
            _mockUserRepository.Setup(r => r.GetUserByGoogleId(googleId)).ReturnsAsync(new User { UserId = 1 });
            _mockUserRepository.Setup(r => r.GetPendingFileSharesForUserAsync(1)).ReturnsAsync(pendingShares);

            // Act
            var result = await _userService.GetPendingSharesAsync(googleId);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddFileToUserAsync_ShouldAddFile_WhenFileShareAccepted()
        {
            // Arrange
            var googleId = "googleId123";
            var fileId = "fileId123";

            // Create a shareRequest to return for the mocked method
            var shareRequest = new PendingFileShare
            {
                MongoFileId = "mongoFileId123",
                FileName = "file1.txt",
                IsAccepted = true
            };

            // Mock the repository call for file share and user
            _mockUserRepository.Setup(r => r.GetUserByGoogleId(googleId)).ReturnsAsync(new User { UserId = 1 });

            // Here we are mocking the method GetFileShareById to simulate it returning an accepted share request
            _mockUserRepository.Setup(r => r.GetFileShareById(fileId)).ReturnsAsync(shareRequest);

            // Act
            await _userService.AddFileToUserAsync(googleId, fileId);

            // Assert
            // Verifying that the AddUserFile method was called with the correct parameters
            _mockUserRepository.Verify(r => r.AddUserFile(It.Is<UserFile>(uf => uf.MongoFileId == "mongoFileId123" && uf.FileName == "file1.txt")), Times.Once);
        }


        [Fact]
        public async Task GetFileByIdAsync_ShouldThrowException_WhenFileNotFound()
        {
            // Arrange
            var fileId = "fileId123";
            _mockUserRepository.Setup(r => r.GetFileByMongoFileId(fileId)).ReturnsAsync((UserFile)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _userService.GetFileByIdAsync(fileId));
        }
    }
}
