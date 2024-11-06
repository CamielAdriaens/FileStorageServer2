using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FileStorage;
using Models;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.TestHost;
namespace IntegrationTests
{

    public class FilesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public FilesControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetUserFiles_ShouldReturnOk_WhenUserHasFiles()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "your_jwt_token");

            // Act
            var response = await _client.GetAsync("/api/files/secure-files");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var files = JsonConvert.DeserializeObject<List<UserFile>>(await response.Content.ReadAsStringAsync());
            Assert.NotNull(files);
        }

        [Fact]
        public async Task UploadFile_ShouldReturnOk_WhenFileIsUploaded()
        {
            // Arrange
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(new MemoryStream(new byte[100])), "file", "testFile.txt");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "your_jwt_token");

            // Act
            var response = await _client.PostAsync("/api/files/upload", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DownloadFile_ShouldReturnFileContent_WhenFileExists()
        {
            // Arrange
            var fileId = "mongo_file_id_here";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "your_jwt_token");

            // Act
            var response = await _client.GetAsync($"/api/files/download/{fileId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/octet-stream", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task DeleteFile_ShouldReturnNoContent_WhenFileIsDeleted()
        {
            // Arrange
            var fileId = "mongo_file_id_here";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "your_jwt_token");

            // Act
            var response = await _client.DeleteAsync($"/api/files/delete/{fileId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}