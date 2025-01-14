using Xunit;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using INTERFACES;
using MODELS;
using Microsoft.Extensions.Logging;
using DTOs;
using FileStorage.Controllers;
using Microsoft.AspNetCore.Mvc;
using LOGIC;
using FileStorage;
using Microsoft.AspNetCore.SignalR;

namespace FileStorage.Tests
{
    public class AlternativeFlowsErrorHandling
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IFileService> _mockFileService;
        private readonly Mock<IFileRepository> _mockFileRepository;
        private readonly FilesController _controller;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IHubContext<FileSharingHub>> _mockHubContext;  // Mock IHubContext

        public AlternativeFlowsErrorHandling()
        {
            _mockUserService = new Mock<IUserService>();
            _mockFileService = new Mock<IFileService>();
            _mockFileRepository = new Mock<IFileRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockHubContext = new Mock<IHubContext<FileSharingHub>>();  // Initialize the mock HubContext
            _controller = new FilesController(_mockFileService.Object, _mockUserService.Object, _mockHubContext.Object);  // Pass it into the controller
        }
        [Fact]
        public async Task GetUserByEmail_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            // Arrange
            var email = "nonexistent-email@example.com";
            _mockUserRepository.Setup(repo => repo.GetUserByEmail(email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _mockUserRepository.Object.GetUserByEmail(email);

            // Assert
            Assert.Null(result);
        }
        [Fact]
        public async Task ShareFileAsync_ShouldNotAddFileShare_WhenRecipientDoesNotExist()
        {
            // Arrange
            var senderGoogleId = "google123";
            var recipientEmail = "nonexistent@example.com";
            var fileName = "file.txt";
            var mongoFileId = "mongoFile123";

            // Mock GetUserByEmail to return null for the nonexistent recipient
            _mockUserRepository.Setup(repo => repo.GetUserByEmail(recipientEmail))
                .ReturnsAsync((User)null);

            // Create the service with the mocked repository
            var userService = new UserService(_mockUserRepository.Object);

            // Act & Assert
            // Expect the ShareFileAsync to throw an exception because the recipient doesn't exist
            await Assert.ThrowsAsync<Exception>(() => userService.ShareFileAsync(senderGoogleId, recipientEmail, fileName, mongoFileId));
        }


        [Fact]
        public async Task GetUserByGoogleId_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var googleId = "google123";
            _mockUserRepository.Setup(repo => repo.GetUserByGoogleId(googleId))
                .ThrowsAsync(new System.Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => _mockUserRepository.Object.GetUserByGoogleId(googleId));
        }
        [Fact]
        public async Task GetUserByEmail_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var email = "test@example.com";
            _mockUserRepository.Setup(repo => repo.GetUserByEmail(email))
                .ThrowsAsync(new System.Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => _mockUserRepository.Object.GetUserByEmail(email));
        }



        [Fact]
        public async Task GetUserByGoogleId_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var googleId = "invalid-google-id";
            _mockUserRepository.Setup(repo => repo.GetUserByGoogleId(googleId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _mockUserRepository.Object.GetUserByGoogleId(googleId);

            // Assert
            Assert.Null(result);
        }


        [Fact]
        public async Task UploadFile_ShouldReturnBadRequest_WhenFileIsNull()
        {
            // Arrange
            var file = (IFormFile)null;

            // Act
            var result = await _controller.UploadFile(file);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadFile_ShouldReturnUnauthorized_WhenGoogleIdIsNotFound()
        {
            // Arrange
            var mockContext = new DefaultHttpContext();
            mockContext.User = null; // No user claims
            _controller.ControllerContext.HttpContext = mockContext;

            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(10);

            // Act
            var result = await _controller.UploadFile(file.Object);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DownloadFile_ShouldReturnBadRequest_WhenFileIdIsInvalid()
        {
            // Arrange
            string invalidFileId = "invalid-id";

            // Act
            var result = await _controller.DownloadFile(invalidFileId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

       
   
    }
}
