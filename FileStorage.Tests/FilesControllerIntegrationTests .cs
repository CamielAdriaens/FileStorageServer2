using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using FileStorage;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Security.Claims;
using FileStorage.Controllers;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using FileStorage.Tests.Mocks;

namespace FileStorage.Tests
{
    public class FilesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly string _jwtToken;

        public FilesControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();

            // Generate a JWT for testing using the AuthController's GenerateJwtToken method
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "testGoogleId"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "Test User")
        };
            var authController = new AuthController(new ConfigurationStub()); // Replace with your IConfiguration setup
            _jwtToken = authController.GenerateJwtToken(claims);
        }

        [Fact]
        public async Task GetUserFiles_ShouldReturnOk_WhenUserHasFiles()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            // Act
            var response = await _client.GetAsync("/api/files/secure-files");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Additional tests for file upload, download, and deletion go here
    }
}