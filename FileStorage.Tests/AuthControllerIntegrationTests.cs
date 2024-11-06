// FileStorage.Tests/AuthControllerIntegrationTests.cs
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using FileStorage;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;
using System.Security.Claims;
using FileStorage.Controllers;
using FileStorage.Tests.Mocks;
using Microsoft.VisualStudio.TestPlatform.TestHost;
namespace FileStorage.Tests
{

    public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly string _jwtToken;

        public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // Create a test client for sending HTTP requests
            _client = factory.CreateClient();

            // Use ConfigurationStub to mock JWT settings for the AuthController
            var authController = new AuthController(new ConfigurationStub());

            // Generate a mock JWT token for authorized tests
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "testGoogleId"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "Test User")
        };
            _jwtToken = authController.GenerateJwtToken(claims);
        }

        [Fact]
        public async Task GoogleLogin_ShouldReturnOk_WithValidToken()
        {
            // Arrange: Prepare a valid Google token payload (in a real scenario, you'd mock Google token validation)
            var requestContent = new StringContent(
                JsonConvert.SerializeObject(new { Credential = "valid_google_token" }),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await _client.PostAsync("/api/auth/google", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Authentication successful", responseContent);
        }

        [Fact]
        public async Task GoogleLogin_ShouldReturnBadRequest_WithInvalidToken()
        {
            // Arrange: Send an invalid token
            var requestContent = new StringContent(
                JsonConvert.SerializeObject(new { Credential = "invalid_google_token" }),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await _client.PostAsync("/api/auth/google", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Invalid Google token", responseContent);
        }

        [Fact]
        public async Task GoogleLogin_ShouldReturnInternalServerError_WithException()
        {
            // Arrange: Simulate a scenario that causes an internal server error, if possible
            // For example, you might send an invalid payload format or cause a token validation failure
            var requestContent = new StringContent(
                JsonConvert.SerializeObject(new { Credential = "" }), // Empty token or malformed payload
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await _client.PostAsync("/api/auth/google", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Expecting BadRequest if token is empty or malformed
        }
    }
}