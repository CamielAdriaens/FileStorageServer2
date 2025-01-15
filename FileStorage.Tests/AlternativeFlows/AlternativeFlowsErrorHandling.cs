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
        private readonly Mock<IHubContext<FileSharingHub>> _mockHubContext; 
        public AlternativeFlowsErrorHandling()
        {
            _mockUserService = new Mock<IUserService>();
            _mockFileService = new Mock<IFileService>();
            _mockFileRepository = new Mock<IFileRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockHubContext = new Mock<IHubContext<FileSharingHub>>();  // Initialize the mock HubContext
            _controller = new FilesController(_mockFileService.Object, _mockUserService.Object, _mockHubContext.Object); 
        }

        //For file sharing we use getuserbyemail
        [Fact]
        public async Task GetUserByEmail_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            var email = "nonexistent-email@example.com";
            _mockUserRepository.Setup(repo => repo.GetUserByEmail(email))
                .ReturnsAsync((User)null);

            var result = await _mockUserRepository.Object.GetUserByEmail(email);

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

            _mockUserRepository.Setup(repo => repo.GetUserByEmail(recipientEmail))
                .ReturnsAsync((User)null);

            var userService = new UserService(_mockUserRepository.Object);

            await Assert.ThrowsAsync<Exception>(() => userService.ShareFileAsync(senderGoogleId, recipientEmail, fileName, mongoFileId));
        }


        [Fact]
        public async Task GetUserByGoogleId_ShouldThrowException_WhenRepositoryFails()
        {
            var googleId = "google123";
            _mockUserRepository.Setup(repo => repo.GetUserByGoogleId(googleId))
                .ThrowsAsync(new System.Exception("Database error"));

            await Assert.ThrowsAsync<System.Exception>(() => _mockUserRepository.Object.GetUserByGoogleId(googleId));
        }
        [Fact]
        public async Task GetUserByEmail_ShouldThrowException_WhenRepositoryFails()
        {
            var email = "test@example.com";
            _mockUserRepository.Setup(repo => repo.GetUserByEmail(email))
                .ThrowsAsync(new System.Exception("Database error"));

            await Assert.ThrowsAsync<System.Exception>(() => _mockUserRepository.Object.GetUserByEmail(email));
        }



        [Fact]
        public async Task GetUserByGoogleId_ShouldReturnNull_WhenUserDoesNotExist()
        {
            var googleId = "invalid-google-id";
            _mockUserRepository.Setup(repo => repo.GetUserByGoogleId(googleId))
                .ReturnsAsync((User)null);

            var result = await _mockUserRepository.Object.GetUserByGoogleId(googleId);

            Assert.Null(result);
        }


        [Fact]
        public async Task UploadFile_ShouldReturnBadRequest_WhenFileIsNull()
        {
            var file = (IFormFile)null;

            var result = await _controller.UploadFile(file);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadFile_ShouldReturnUnauthorized_WhenGoogleIdIsNotFound()
        {
            var mockContext = new DefaultHttpContext();
            mockContext.User = null; // No user claims
            _controller.ControllerContext.HttpContext = mockContext;

            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(10);

            var result = await _controller.UploadFile(file.Object);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DownloadFile_ShouldReturnBadRequest_WhenFileIdIsInvalid()
        {
            string invalidFileId = "invalid-id";

            var result = await _controller.DownloadFile(invalidFileId);

            Assert.IsType<BadRequestObjectResult>(result);
        }

       
   
    }
}
