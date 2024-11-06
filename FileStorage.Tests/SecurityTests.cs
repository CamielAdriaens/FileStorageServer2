using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using FileStorage;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using FileStorage.Controllers;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace FileStorage.Tests
{

    public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly string _validJwtToken;
        private readonly string _expiredJwtToken;

        public SecurityTests(WebApplicationFactory<Program> factory)
        {
            var scopeFactory = factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();

            // Generate a valid JWT for testing
            var authController = scope.ServiceProvider.GetRequiredService<AuthController>();
            var validClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "testGoogleId"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "Test User")
        };
            _validJwtToken = authController.GenerateJwtToken(validClaims);

            // Generate an expired JWT for testing unauthorized access
            var expiredClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "testGoogleId"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "Test User")
        };
            _expiredJwtToken = authController.GenerateJwtToken(expiredClaims); // Set up to create an expired token if possible

            _client = factory.CreateClient();
        }

        [Fact]
        public async Task FilesController_ShouldReturnUnauthorized_WhenTokenIsInvalid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid_jwt_token");

            // Act
            var response = await _client.GetAsync("/api/files/secure-files");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task FilesController_ShouldReturnUnauthorized_WhenTokenIsExpired()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _expiredJwtToken);

            // Act
            var response = await _client.GetAsync("/api/files/secure-files");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task FilesController_ShouldReturnForbidden_WhenUserDoesNotOwnFile()
        {
            // Arrange
            var otherUserFileId = "file_id_of_different_user";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _validJwtToken);

            // Act
            var response = await _client.GetAsync($"/api/files/download/{otherUserFileId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task FilesController_ShouldReturnOk_WhenTokenIsValid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _validJwtToken);

            // Act
            var response = await _client.GetAsync("/api/files/secure-files");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}