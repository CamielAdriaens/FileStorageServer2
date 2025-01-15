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
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(config => config["JwtSettings:SecretKey"]).Returns("dummySecretKey");
            _configurationMock.Setup(config => config["JwtSettings:Issuer"]).Returns("dummyIssuer");
            _configurationMock.Setup(config => config["JwtSettings:Audience"]).Returns("dummyAudience");

            _authController = new AuthController(_configurationMock.Object);
        }

        [Fact]
        public async Task GoogleLogin_ShouldReturnUnauthorized_WhenTokenIsInvalid()
        {
            var invalidTokenRequest = new GoogleTokenRequest { Credential = "invalid_token" };

            var result = await _authController.GoogleLogin(invalidTokenRequest) as ObjectResult;

            
            Assert.Equal(400, result?.StatusCode);
        }
    }
}
