using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using DTOs;
using FileStorage.Controllers;
using INTERFACES;
using MODELS;
using Xunit;
using MongoDB.Bson;

namespace FileStorage.Tests
{
    public class FilesControllerTests
    {
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IHubContext<FileSharingHub>> _hubContextMock;
        private readonly FilesController _controller;

        public FilesControllerTests()
        {
            _fileServiceMock = new Mock<IFileService>();
            _userServiceMock = new Mock<IUserService>();
            _hubContextMock = new Mock<IHubContext<FileSharingHub>>();
            _controller = new FilesController(_fileServiceMock.Object, _userServiceMock.Object, _hubContextMock.Object);

            // Ensure the controller has a valid user context
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, "google123"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.Name, "Test User")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetUserFiles_ShouldReturnOk_WhenFilesExist()
        {
            // Arrange
            var userFiles = new List<UserFile>
            {
                new UserFile { FileName = "file1.txt", MongoFileId = "file123" },
                new UserFile { FileName = "file2.txt", MongoFileId = "file456" }
            };

            _userServiceMock.Setup(s => s.GetUserFilesAsync("google123")).ReturnsAsync(userFiles);

            // Act
            var result = await _controller.GetUserFiles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedFiles = Assert.IsType<List<UserFile>>(okResult.Value);
            Assert.Equal(2, returnedFiles.Count);
        }

        [Fact]
        public async Task DownloadFile_ShouldReturnFile_WhenFileExists()
        {
            // Arrange
            var fileId = "507f191e810c19729de860ea"; // Valid ObjectId string (24 hex characters)
            var objectId = new ObjectId(fileId);    // This will not throw FormatException
            var googleId = "google123";

            var mockUserFiles = new List<UserFile>
    {
        new UserFile { MongoFileId = fileId, FileName = "test.txt" }
    };

            _userServiceMock.Setup(s => s.GetUserFilesAsync(googleId))
                            .ReturnsAsync(mockUserFiles);
            _fileServiceMock.Setup(s => s.DownloadFileAsync(objectId))
                            .ReturnsAsync(new MemoryStream());  // Simulate file download stream

            // Act
            var result = await _controller.DownloadFile(fileId);

            // Assert
            var fileStreamResult = Assert.IsType<FileStreamResult>(result);
            Assert.NotNull(fileStreamResult.FileStream);
            Assert.Equal("application/octet-stream", fileStreamResult.ContentType);
            Assert.Equal("test.txt", fileStreamResult.FileDownloadName);
        }

        [Fact]
        public async Task DeleteFile_ShouldReturnOk_WhenFileIsDeleted()
        {
            // Arrange
            var fileId = "507f191e810c19729de860ea"; // Valid ObjectId string (24 hex characters)
            var objectId = new ObjectId(fileId);    // This will not throw FormatException
            var googleId = "google123";

            var mockUserFiles = new List<UserFile>
    {
        new UserFile { MongoFileId = fileId, FileName = "test.txt" }
    };

            _userServiceMock.Setup(s => s.GetUserFilesAsync(googleId))
                            .ReturnsAsync(mockUserFiles);
            _fileServiceMock.Setup(s => s.DeleteFileAsync(objectId))
                            .Returns(Task.CompletedTask);
            _userServiceMock.Setup(s => s.RemoveUserFileAsync(googleId, fileId))
                            .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteFile(fileId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("File Deleted Succesfully", okResult.Value);
        }


        [Fact]
        public async Task GetPendingShares_ShouldReturnOk_WhenSharesExist()
        {
            // Arrange
            var pendingShares = new List<PendingFileShare>
            {
                new PendingFileShare { FileName = "file1.txt", MongoFileId = "file123" },
                new PendingFileShare { FileName = "file2.txt", MongoFileId = "file456" }
            };

            _userServiceMock.Setup(s => s.GetPendingSharesAsync("google123")).ReturnsAsync(pendingShares);

            // Act
            var result = await _controller.GetPendingShares();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedShares = Assert.IsType<List<PendingFileShare>>(okResult.Value);
            Assert.Equal(2, returnedShares.Count);
        }
        [Fact]
        public async Task UploadFile_ShouldReturnOk_WhenFileIsUploaded()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var content = "Test content";
            var fileName = "test.txt";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            // Write some content to the stream
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            // Set up mock properties
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream); // Ensure a valid stream is returned
            mockFile.Setup(f => f.FileName).Returns(fileName);       // Ensure a valid file name is returned
            mockFile.Setup(f => f.Length).Returns(stream.Length);    // Ensure the length is set correctly

            // Mock the user service and file service behaviors
            var mockUser = new User { UserId = 1, GoogleId = "google123" };
            _userServiceMock.Setup(s => s.GetOrCreateUserByGoogleIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .ReturnsAsync(mockUser);

            var objectId = new ObjectId("507f191e810c19729de860ea");
            _fileServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                            .ReturnsAsync(objectId); // Simulate the file upload and returning an ObjectId

            _userServiceMock.Setup(s => s.AddUserFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .Returns(Task.CompletedTask); // Simulate adding the file to the user

            // Act
            var result = await _controller.UploadFile(mockFile.Object);

            // Assert: Ensure the result is OkObjectResult and that it contains the correct fileId
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedValue = Assert.IsType<dynamic>(okResult.Value);
            Assert.Equal(objectId.ToString(), returnedValue.FileId);

            // Handle any potential failure (e.g., if UploadFileAsync returned null)
            var badRequestResult = result as BadRequestObjectResult;
            if (badRequestResult != null)
            {
                Assert.Equal("File upload failed.", badRequestResult.Value);
            }
        }

        [Fact]
        public async Task ShareFile_ShouldReturnOk_WhenFileIsShared()
        {
            // Arrange: Create mock claims for the current user
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "google123"), // Mock Google ID
        new Claim(ClaimTypes.Email, "user@example.com"),   // Mock user email
        new Claim(ClaimTypes.Name, "Test User")            // Mock user name
    };

            // Create a ClaimsPrincipal and set it in the controller's context
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Create the request to share a file
            var shareRequest = new ShareFileRequest
            {
                RecipientEmail = "otheruser@example.com",
                FileName = "test.txt",
                MongoFileId = "file123"
            };

            // Mock the ShareFileAsync method to simulate a successful file share
            _userServiceMock.Setup(s => s.ShareFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .Returns(Task.CompletedTask); // Simulate successful sharing

            // Act: Call the ShareFile method
            var result = await _controller.ShareFile(shareRequest);

            // Assert: Verify that we get an OkObjectResult with the expected success message
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("File shared successfully.", okResult.Value);

            // Handle the case if the ShareFileAsync method is mocked to return BadRequest (in case of error)
            var badRequestResult = result as BadRequestObjectResult;
            if (badRequestResult != null)
            {
                Assert.Equal("File share failed.", badRequestResult.Value);
            }
        }

        [Fact]
        public async Task AcceptShare_ShouldReturnOk_WhenShareIsAccepted()
        {
            // Arrange
            _userServiceMock.Setup(s => s.AcceptFileShareAsync(1));

            // Act
            var result = await _controller.AcceptShare(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("File share accepted.", okResult.Value);
        }

        [Fact]
        public async Task RefuseShare_ShouldReturnOk_WhenShareIsRefused()
        {
            // Arrange
            _userServiceMock.Setup(s => s.RefuseFileShareAsync(1));

            // Act
            var result = await _controller.RefuseShare(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("File share refused.", okResult.Value);
        }
    }
}
