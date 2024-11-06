using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using INTERFACES;
using LOGIC;
using Models;

namespace FileStorage.Tests
{
    public class UserServiceTests
    {
        private readonly UserService _userService;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetOrCreateUserByGoogleIdAsync_ShouldCreateNewUser_WhenUserDoesNotExist()
        {
            // Arrange
            var googleId = "testGoogleId";
            var email = "test@example.com";
            var name = "Test User";

            _userRepositoryMock.Setup(repo => repo.GetUserByGoogleId(googleId)).ReturnsAsync((User)null);
            _userRepositoryMock.Setup(repo => repo.CreateUser(It.IsAny<User>())).ReturnsAsync(new User { GoogleId = googleId, Email = email, Name = name });

            // Act
            var result = await _userService.GetOrCreateUserByGoogleIdAsync(googleId, email, name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(googleId, result.GoogleId);
            _userRepositoryMock.Verify(repo => repo.CreateUser(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task GetUserFilesAsync_ShouldReturnUserFiles_WhenUserExists()
        {
            // Arrange
            var googleId = "testGoogleId";
            var userFiles = new List<UserFile> { new UserFile { MongoFileId = "fileId1", FileName = "file1.txt" } };

            _userRepositoryMock.Setup(repo => repo.GetUserFiles(googleId)).ReturnsAsync(userFiles);

            // Act
            var result = await _userService.GetUserFilesAsync(googleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userFiles.Count, result.Count);
        }

        [Fact]
        public async Task AddUserFileAsync_ShouldAddFileToUser()
        {
            // Arrange
            var googleId = "testGoogleId";
            var mongoFileId = "mongoFileId123";
            var fileName = "testFile.txt";
            var user = new User { GoogleId = googleId };

            _userRepositoryMock.Setup(repo => repo.GetUserByGoogleId(googleId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.AddUserFile(It.IsAny<UserFile>())).Returns(Task.CompletedTask);

            // Act
            await _userService.AddUserFileAsync(googleId, mongoFileId, fileName);

            // Assert
            _userRepositoryMock.Verify(repo => repo.AddUserFile(It.Is<UserFile>(f => f.FileName == fileName && f.MongoFileId == mongoFileId)), Times.Once);
        }

        [Fact]
        public async Task RemoveUserFileAsync_ShouldRemoveFileFromUser()
        {
            // Arrange
            var googleId = "testGoogleId";
            var mongoFileId = "mongoFileId123";
            var user = new User { Id = 1, GoogleId = googleId };

            _userRepositoryMock.Setup(repo => repo.GetUserByGoogleId(googleId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.RemoveUserFile(user.Id, mongoFileId)).Returns(Task.CompletedTask);

            // Act
            await _userService.RemoveUserFileAsync(googleId, mongoFileId);

            // Assert
            _userRepositoryMock.Verify(repo => repo.RemoveUserFile(user.Id, mongoFileId), Times.Once);
        }
    }
}