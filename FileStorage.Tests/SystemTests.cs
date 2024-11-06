using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FileStorage;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using FileStorage.Controllers;
using System.Security.Claims;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace FileStorage.Tests
{
    public class SystemTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly string _jwtToken;

        public SystemTests(WebApplicationFactory<Program> factory)
        {
            var scopeFactory = factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();

            // Generate a JWT for testing using the AuthController's method
            var authController = scope.ServiceProvider.GetRequiredService<AuthController>();
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "testGoogleId"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "Test User")
        };
            _jwtToken = authController.GenerateJwtToken(claims);

            _client = factory.CreateClient();
        }

        [Fact]
        public async Task FileManagementWorkflow_ShouldCompleteEndToEnd_WhenAuthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            // Step 1: Upload a File
            var uploadContent = new MultipartFormDataContent();
            uploadContent.Add(new StreamContent(new MemoryStream(new byte[100])), "file", "testFile.txt");
            var uploadResponse = await _client.PostAsync("/api/files/upload", uploadContent);

            Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);
            var uploadResult = JsonConvert.DeserializeObject<dynamic>(await uploadResponse.Content.ReadAsStringAsync());
            string fileId = uploadResult.FileId;
            Assert.NotEmpty(fileId);

            // Step 2: Download the File
            var downloadResponse = await _client.GetAsync($"/api/files/download/{fileId}");
            Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);
            Assert.Equal("application/octet-stream", downloadResponse.Content.Headers.ContentType.ToString());

            // Step 3: Delete the File
            var deleteResponse = await _client.DeleteAsync($"/api/files/delete/{fileId}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }
    }
}