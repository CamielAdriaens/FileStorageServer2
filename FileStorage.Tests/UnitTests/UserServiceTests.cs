using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using INTERFACES;
using LOGIC;
using MODELS;

namespace FileStorage.Tests.UnitTests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetOrCreateUserByGoogleIdAsync_ShouldReturnExistingUser()
        {
            var googleId = "google-id";
            var user = new User { GoogleId = googleId, Email = "test@example.com", Name = "Test User" };
            _userRepositoryMock.Setup(repo => repo.GetUserByGoogleId(googleId)).ReturnsAsync(user);

            var result = await _userService.GetOrCreateUserByGoogleIdAsync(googleId, "test@example.com", "Test User");

            Assert.Equal(user, result);
            _userRepositoryMock.Verify(repo => repo.CreateUser(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetOrCreateUserByGoogleIdAsync_ShouldCreateNewUser()
        {
            var googleId = "new-google-id";
            User user = null;
            _userRepositoryMock.Setup(repo => repo.GetUserByGoogleId(googleId)).ReturnsAsync(user);

            var newUser = new User { GoogleId = googleId, Email = "new@example.com", Name = "New User" };
            _userRepositoryMock.Setup(repo => repo.CreateUser(It.IsAny<User>())).ReturnsAsync(newUser);

            var result = await _userService.GetOrCreateUserByGoogleIdAsync(googleId, "new@example.com", "New User");

            Assert.Equal(newUser, result);
            _userRepositoryMock.Verify(repo => repo.CreateUser(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task GetUserFilesAsync_ShouldReturnUserFiles()
        {
            var googleId = "google-id";
            var files = new List<UserFile> { new UserFile { MongoFileId = "1", FileName = "file1.txt" } };
            _userRepositoryMock.Setup(repo => repo.GetUserFiles(googleId)).ReturnsAsync(files);

            var result = await _userService.GetUserFilesAsync(googleId);

            Assert.Equal(files, result);
        }

        [Fact]
        public async Task AddUserFileAsync_ShouldAddUserFile()
        {
            var googleId = "google-id";
            var mongoFileId = "mongo-file-id";
            var fileName = "file.txt";
            var user = new User { UserId = 1, GoogleId = googleId };
            _userRepositoryMock.Setup(repo => repo.GetUserByGoogleId(googleId)).ReturnsAsync(user);

            await _userService.AddUserFileAsync(googleId, mongoFileId, fileName);

            _userRepositoryMock.Verify(repo => repo.AddUserFile(It.Is<UserFile>(f => f.MongoFileId == mongoFileId && f.FileName == fileName)), Times.Once);
        }

        [Fact]
        public async Task RemoveUserFileAsync_ShouldRemoveUserFile()
        {
            var googleId = "google-id";
            var mongoFileId = "mongo-file-id";
            var user = new User { UserId = 1, GoogleId = googleId };
            _userRepositoryMock.Setup(repo => repo.GetUserByGoogleId(googleId)).ReturnsAsync(user);

            await _userService.RemoveUserFileAsync(googleId, mongoFileId);

            _userRepositoryMock.Verify(repo => repo.RemoveUserFile(user.UserId, mongoFileId), Times.Once);
        }
    }
}