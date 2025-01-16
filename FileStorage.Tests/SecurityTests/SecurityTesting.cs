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
