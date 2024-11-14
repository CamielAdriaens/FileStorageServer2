using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Xunit;
using DAL;
using MODELS;
using Microsoft.Extensions.Options;
using Mongo2Go;
using System.Collections.Generic;

namespace FileStorage.Tests.IntegrationTests
{
    public class FileRepositoryIntegrationTests : IAsyncLifetime
    {
        private readonly MongoDbRunner _mongoRunner;
        private readonly IMongoDatabase _database;
        private readonly FileRepository _fileRepository;

        public FileRepositoryIntegrationTests()
        {
            // Initialize an in-memory MongoDB instance
            _mongoRunner = MongoDbRunner.Start();
            var client = new MongoClient(_mongoRunner.ConnectionString);
            _database = client.GetDatabase("TestDatabase");

            // Setup repository with in-memory MongoDB database
            var settings = Options.Create(new MongoDbSettings
            {
                ConnectionString = _mongoRunner.ConnectionString,
                DatabaseName = "TestDatabase"
            });
            _fileRepository = new FileRepository(settings);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _database.DropCollectionAsync("fs.files");
            await _database.DropCollectionAsync("fs.chunks");
            _mongoRunner.Dispose();
        }

        [Fact]
        public async Task UploadFileAsync_ShouldUploadFile()
        {
            // Arrange
            var fileName = "test.txt";
            var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test content"));

            // Act
            var fileId = await _fileRepository.UploadFileAsync(content, fileName);

            // Assert
            var files = await _fileRepository.GetFilesAsync();
            Assert.Contains(files, f => f.Id == fileId);
        }

        [Fact]
        public async Task GetFilesAsync_ShouldReturnFiles()
        {
            // Arrange
            var fileName = "test.txt";
            var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test content"));
            await _fileRepository.UploadFileAsync(content, fileName);

            // Act
            var files = await _fileRepository.GetFilesAsync();

            // Assert
            Assert.Single(files);
            Assert.Equal(fileName, files[0].Filename);
        }

        [Fact]
        public async Task DownloadFileAsync_ShouldReturnFileStream()
        {
            // Arrange
            var fileName = "test.txt";
            var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test content"));
            var fileId = await _fileRepository.UploadFileAsync(content, fileName);

            // Act
            var downloadedStream = await _fileRepository.DownloadFileAsync(fileId);

            // Assert
            downloadedStream.Position = 0;
            using var reader = new StreamReader(downloadedStream);
            var fileContent = await reader.ReadToEndAsync();
            Assert.Equal("Test content", fileContent);
        }

        [Fact]
        public async Task DeleteFileAsync_ShouldRemoveFile()
        {
            // Arrange
            var fileName = "test.txt";
            var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test content"));
            var fileId = await _fileRepository.UploadFileAsync(content, fileName);

            // Act
            await _fileRepository.DeleteFileAsync(fileId);
            var files = await _fileRepository.GetFilesAsync();

            // Assert
            Assert.DoesNotContain(files, f => f.Id == fileId);
        }
    }
}