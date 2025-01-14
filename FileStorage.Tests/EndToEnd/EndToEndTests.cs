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
using FileStorage.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using DTOs;
using Microsoft.AspNetCore.SignalR;

namespace FileStorage.Tests
{
    public class EndToEndTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IFileService> _mockFileService;
        private readonly Mock<IFileRepository> _mockFileRepository;
        private readonly FilesController _controller;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IHubContext<FileSharingHub>> _mockHubContext;  // Mock IHubContext

        public EndToEndTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockFileService = new Mock<IFileService>();
            _mockFileRepository = new Mock<IFileRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockHubContext = new Mock<IHubContext<FileSharingHub>>();  // Initialize the mock HubContext
            _controller = new FilesController(_mockFileService.Object, _mockUserService.Object, _mockHubContext.Object);  // Pass it into the controller
        }

        [Fact]
        public async Task EndToEnd_FileUpload_ShouldWorkSuccessfully()
        {
            // Arrange: Setup mock user and file service
            var googleId = "testGoogleId";
            var email = "test@example.com";
            var name = "Test User";

            // Generate an ObjectId for the file ID
            var objectId = ObjectId.GenerateNewId(); // This will generate an ObjectId for the test

            var mockContext = new DefaultHttpContext();
            mockContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                new[]
                {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, googleId),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, name)
                }));
            _controller.ControllerContext.HttpContext = mockContext;

            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(10);
            file.Setup(f => f.FileName).Returns("testFile.txt");
            var stream = new MemoryStream(new byte[10]);
            file.Setup(f => f.OpenReadStream()).Returns(stream);

            _mockUserService.Setup(us => us.GetOrCreateUserByGoogleIdAsync(googleId, email, name))
                            .ReturnsAsync(new User { GoogleId = googleId, Email = email, Name = name });
            _mockFileService.Setup(fs => fs.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                            .ReturnsAsync(objectId); // Mock the return value to return ObjectId as file ID
            _mockUserService.Setup(us => us.AddUserFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .Returns(Task.CompletedTask);

            // Act: Upload file
            var result = await _controller.UploadFile(file.Object);
            Assert.IsType<OkObjectResult>(result);

            // Assert: File uploaded and added to user
            var fileId = ((OkObjectResult)result).Value; // Extract the fileId from the result
            Assert.NotNull(fileId);

            // Extract the FileId correctly
            var fileIdString = fileId?.ToString() ?? string.Empty;

            Assert.Equal(objectId.ToString(), fileIdString); // Ensure the file ID matches
        }

        [Fact]
        public async Task EndToEnd_FileDelete_ShouldWorkSuccessfully()
        {
            // Arrange: Setup mock user and file service
            var googleId = "testGoogleId";
            var email = "test@example.com";
            var name = "Test User";

            // Generate an ObjectId for the file ID
            var objectId = ObjectId.GenerateNewId(); // Generate a new ObjectId for the file to be deleted

            var mockContext = new DefaultHttpContext();
            mockContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                new[]
                {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, googleId),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, name)
                }));
            _controller.ControllerContext.HttpContext = mockContext;

            // Setup mock behavior for file retrieval and deletion
            _mockFileService.Setup(fs => fs.DeleteFileAsync(It.Is<ObjectId>(id => id == objectId)))
                            .Returns(Task.CompletedTask); // Mock the delete file action

            // Act: Delete file, passing the ObjectId as a string (controller accepts string)
            var result = await _controller.DeleteFile(objectId.ToString()); // Pass ObjectId as string to the controller

            // Assert: File deletion successful (no content expected after deletion)
            Assert.IsType<NoContentResult>(result);
        }


        [Fact]
        public async Task EndToEnd_AddNewUserAndFile_ShouldWorkCorrectly()
        {
            // Arrange
            var googleId = "google123";
            var email = "user@example.com";
            var name = "Test User";
            var mongoFileId = "mongoFile123";
            var fileName = "file.txt";
            var user = new User { GoogleId = googleId, Email = email, Name = name };

            _mockUserRepository.Setup(repo => repo.GetUserByGoogleId(googleId))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(repo => repo.AddUserFile(It.IsAny<UserFile>()))
                .Returns(Task.CompletedTask);

            // Act
            await _mockUserRepository.Object.AddUserFile(new UserFile {MongoFileId = mongoFileId, FileName = fileName });

            // Assert
            _mockUserRepository.Verify(repo => repo.AddUserFile(It.Is<UserFile>(uf => uf.MongoFileId == mongoFileId)), Times.Once);
        }

       
    }
}
