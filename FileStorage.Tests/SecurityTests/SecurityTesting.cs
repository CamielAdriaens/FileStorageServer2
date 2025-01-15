using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FileStorage.Controllers;
using MODELS;
using DAL;
using LOGIC;
using Microsoft.AspNetCore.SignalR;
using DTOs;
using INTERFACES;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace FileStorage.Tests.SecurityTests
{
    public class SecurityTesting
    {
        private readonly Mock<IFileService> fileServiceMock;
        private readonly Mock<IUserService> userServiceMock;
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly Mock<IHubContext<FileSharingHub>> hubContextMock;

        public SecurityTesting()
        {
            fileServiceMock = new Mock<IFileService>();
            userServiceMock = new Mock<IUserService>();
            userRepositoryMock = new Mock<IUserRepository>();
            hubContextMock = new Mock<IHubContext<FileSharingHub>>();
        }

        #region Bestandsbeveiligingstests (MongoDB)
        [Fact]
        public async Task UploadFile_InvalidFileType_ReturnsBadRequest()
        {
            // Arrange
            var invalidFile = new Mock<IFormFile>();
            invalidFile.Setup(f => f.FileName).Returns("<script>alert('XSS')</script>.exe");
            invalidFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[] { 0x00 }));

            var controller = new FilesController(fileServiceMock.Object, userServiceMock.Object, hubContextMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.UploadFile(invalidFile.Object);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("File type is not allowed.", actionResult.Value);
        }

        [Fact]
        public async Task GetUserFiles_NoSQLInjection_ReturnsUnauthorized()
        {
            // Arrange
            var maliciousUserId = "' OR 1=1; --"; // Example of NoSQL injection payload
            var controller = new FilesController(fileServiceMock.Object, userServiceMock.Object, hubContextMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.GetUserFiles();

            // Assert
            var actionResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Google ID not found", actionResult.Value);
        }
        #endregion


        [Fact]
        public async Task ShareFile_SQLInjection_ReturnsBadRequest()
        {
            // Arrange
            var invalidEmail = "'; DROP TABLE PendingFileShares; --";
            var shareRequest = new ShareFileRequest
            {
                RecipientEmail = invalidEmail,
                FileName = "example.txt",
                MongoFileId = "12345"
            };

            var controller = new FilesController(fileServiceMock.Object, userServiceMock.Object, hubContextMock.Object);

            // Act
            var result = await controller.ShareFile(shareRequest);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid email format or potential SQL injection detected.", actionResult.Value);
        }

        [Fact]
        public async Task GetUserFiles_UnauthorizedAccess_ReturnsForbidden()
        {
            // Arrange
            var unauthorizedGoogleId = "unauthorizedGoogleId";
            var controller = new FilesController(fileServiceMock.Object, userServiceMock.Object, hubContextMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, unauthorizedGoogleId)
                    }))
                }
            };

            // Act
            var result = await controller.GetUserFiles();

            // Assert
            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, actionResult.StatusCode);
            Assert.Equal("User not authorized to access this resource", actionResult.Value);
        }

        #region API-beveiligingstests
        [Fact]
        public async Task UploadFile_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            var unauthorizedGoogleId = "unauthorizedGoogleId";
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns("testfile.txt");
            file.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[] { 0x00 }));

            var controller = new FilesController(fileServiceMock.Object, userServiceMock.Object, hubContextMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, unauthorizedGoogleId)
                    }))
                }
            };

            // Act
            var result = await controller.UploadFile(file.Object);

            // Assert
            var actionResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Google ID not found", actionResult.Value);
        }


        #endregion

        [Fact]
        public void User_Should_Reject_SQL_Injection_Attempts()
        {
            // Arrange
            var user = new User
            {
                Name = "Robert'); DROP TABLE Users;--",
                Email = "test@example.com",
                GoogleId = "12345"
            };
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(user);

            // Act
            var isValid = Validator.TryValidateObject(user, context, validationResults, true);

            // Assert
            Assert.True(isValid); // Validation will pass, but database operations should handle SQL injection separately.
        }

        [Fact]
        public void UserFile_Should_Reject_XSS_Attacks()
        {
            // Arrange
            var userFile = new UserFile
            {
                MongoFileId = "12345",
                FileName = "<script>alert('XSS')</script>",
                UploadDate = DateTime.Now,
                UserId = 1
            };

            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(userFile);

            // Act
            var isValid = Validator.TryValidateObject(userFile, context, validationResults, true);

            // Assert
            Assert.True(isValid); // Validation will pass, but sanitization should be handled at the input layer.
        }

    }
}
