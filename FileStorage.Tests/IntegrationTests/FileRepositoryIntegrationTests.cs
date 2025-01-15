using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;
using DAL;
using MODELS;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace FileStorage.Tests.IntegrationTests
{
    public class FileRepositoryIntegrationTests : IAsyncLifetime
    {
        private readonly IMongoDatabase _database;
        private readonly FileRepository _fileRepository;

        public FileRepositoryIntegrationTests()
        {
            var connectionString = "mongodb://localhost:27017"; // Connects to Docker MongoDB service
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("TestDatabase");

            var settings = Options.Create(new MongoDbSettings
            {
                ConnectionString = connectionString,
                DatabaseName = "TestDatabase"
            });
            _fileRepository = new FileRepository(settings);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            // Clean up test data after each test
            await _database.DropCollectionAsync("fs.files");
            await _database.DropCollectionAsync("fs.chunks");
        }

        [Fact]
        public async Task UploadFileAsync_ShouldUploadFile()
        {
            var fileName = "test.txt";
            var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test content"));

            var fileId = await _fileRepository.UploadFileAsync(content, fileName);

            var files = await _fileRepository.GetFilesAsync();
            Assert.Contains(files, f => f.Id == fileId);
        }

        [Fact]
        public async Task GetFilesAsync_ShouldReturnFiles()
        {
            var fileName = "test.txt";
            var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test content"));
            await _fileRepository.UploadFileAsync(content, fileName);

            var files = await _fileRepository.GetFilesAsync();

            Assert.Single(files);
            Assert.Equal(fileName, files[0].Filename);
        }

        [Fact]
        public async Task DownloadFileAsync_ShouldReturnFileStream()
        {
            var fileName = "test.txt";
            var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test content"));
            var fileId = await _fileRepository.UploadFileAsync(content, fileName);

            var downloadedStream = await _fileRepository.DownloadFileAsync(fileId);

            downloadedStream.Position = 0;
            using var reader = new StreamReader(downloadedStream);
            var fileContent = await reader.ReadToEndAsync();
            Assert.Equal("Test content", fileContent);
        }

        [Fact]
        public async Task DeleteFileAsync_ShouldRemoveFile()
        {
            var fileName = "test.txt";
            var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test content"));
            var fileId = await _fileRepository.UploadFileAsync(content, fileName);

            await _fileRepository.DeleteFileAsync(fileId);
            var files = await _fileRepository.GetFilesAsync();

            Assert.DoesNotContain(files, f => f.Id == fileId);
        }
    }
}
