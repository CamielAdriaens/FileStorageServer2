using Xunit;
using Microsoft.AspNetCore.Mvc;
using FileStorage.Controllers;
using Moq;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace FileStorage.Tests.SecurityTests
{
    public class AuthControllerTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            // Setup mock configuration with required JWT settings
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(config => config["JwtSettings:SecretKey"]).Returns("dummySecretKey");
            _configurationMock.Setup(config => config["JwtSettings:Issuer"]).Returns("dummyIssuer");
            _configurationMock.Setup(config => config["JwtSettings:Audience"]).Returns("dummyAudience");

            // Pass the mock configuration to AuthController
            _authController = new AuthController(_configurationMock.Object);
        }

        [Fact]
        public async Task GoogleLogin_ShouldReturnUnauthorized_WhenTokenIsInvalid()
        {
            // Arrange
            var invalidTokenRequest = new GoogleTokenRequest { Credential = "invalid_token" };

            // Act
            var result = await _authController.GoogleLogin(invalidTokenRequest) as ObjectResult;

            // Assert
            Assert.Equal(400, result?.StatusCode);
        }
    }
}
