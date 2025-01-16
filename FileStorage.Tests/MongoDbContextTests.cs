using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;
using DAL;
using MODELS;
using Xunit;
using System;

namespace FileStorage.Tests
{
    public class MongoDbContextIntegrationTests : IAsyncLifetime
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDbContext _mongoDbContext;

        public MongoDbContextIntegrationTests()
        {
            // Connect to MongoDB running locally (or use Docker/MongoDB Atlas for remote)
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("TestDatabase2");

            var settings = Options.Create(new MongoDbSettings
            {
                ConnectionString = connectionString,
                DatabaseName = "TestDatabase2"
            });

            _mongoDbContext = new MongoDbContext(settings);
        }

        // IAsyncLifetime methods for setup and teardown
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            // Clean up fs.files and fs.chunks collections after each test
            await _database.DropCollectionAsync("fs.files");
            await _database.DropCollectionAsync("fs.chunks");
        }

        [Fact]
        public async Task GetFilesAsync_ShouldReturnEmptyList_WhenNoFilesExist()
        {
            // Ensure collection is empty before testing
            await _database.DropCollectionAsync("fs.files");
            await _database.DropCollectionAsync("fs.chunks");

            // Act
            var result = await _mongoDbContext.GetFilesAsync();

            // Assert
            Assert.Empty(result);  // No files should be in the collection
        }

        [Fact]
        public async Task UploadFileAsync_ShouldReturnFileId_WhenFileIsUploaded()
        {
            // Arrange
            var fileName = "testfile.txt";
            var fileContent = "Hello, World!";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(fileContent);
            writer.Flush();
            stream.Position = 0;  // Reset stream position after writing

            // Act
            var fileId = await _mongoDbContext.UploadFileAsync(stream, fileName);

            // Assert
            Assert.IsType<ObjectId>(fileId);  // The file should be uploaded and return an ObjectId
        }

        [Fact]
        public async Task DownloadFileAsync_ShouldReturnFileStream_WhenFileExists()
        {
            // Arrange
            var fileName = "testfile.txt";
            var fileContent = "This is a test file content";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(fileContent);
            writer.Flush();
            stream.Position = 0;  // Reset stream position

            var fileId = await _mongoDbContext.UploadFileAsync(stream, fileName);

            // Act
            var resultStream = await _mongoDbContext.DownloadFileAsync(fileId);

            // Assert
            Assert.IsType<MemoryStream>(resultStream);  // The result should be a MemoryStream
            resultStream.Position = 0;
            using (var reader = new StreamReader(resultStream))
            {
                var content = await reader.ReadToEndAsync();
                Assert.Equal(fileContent, content);  // Assert that the file content matches
            }
        }

        [Fact]
        public async Task DeleteFileAsync_ShouldDeleteFile_WhenFileExists()
        {
            // Arrange
            var fileName = "testfile.txt";
            var fileContent = "File content to delete";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(fileContent);
            writer.Flush();
            stream.Position = 0;

            var fileId = await _mongoDbContext.UploadFileAsync(stream, fileName);

            // Act
            await _mongoDbContext.DeleteFileAsync(fileId);

            // Assert: Verify the file was deleted by attempting to download it (it should fail)
            var exception = await Assert.ThrowsAsync<MongoDB.Driver.GridFS.GridFSFileNotFoundException>(() => _mongoDbContext.DownloadFileAsync(fileId));
            Assert.Contains("GridFS file not found", exception.Message);  // Ensure the correct exception type is caught
        }

        [Fact]
        public async Task GetFilesAsync_ShouldReturnListOfFiles_WhenFilesExist()
        {
            // Arrange
            var fileName1 = "file1.txt";
            var fileContent1 = "First file content";
            var stream1 = new MemoryStream();
            var writer1 = new StreamWriter(stream1);
            writer1.Write(fileContent1);
            writer1.Flush();
            stream1.Position = 0;

            var fileName2 = "file2.txt";
            var fileContent2 = "Second file content";
            var stream2 = new MemoryStream();
            var writer2 = new StreamWriter(stream2);
            writer2.Write(fileContent2);
            writer2.Flush();
            stream2.Position = 0;

            await _mongoDbContext.UploadFileAsync(stream1, fileName1);
            await _mongoDbContext.UploadFileAsync(stream2, fileName2);

            // Act
            var files = await _mongoDbContext.GetFilesAsync();

            // Assert
            Assert.Equal(2, files.Count);  // Two files should exist in the collection
        }

        [Fact]
        public async Task UploadFileAsync_ShouldThrowArgumentException_WhenFileNameIsEmpty()
        {
            // Arrange
            var stream = new MemoryStream();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mongoDbContext.UploadFileAsync(stream, ""));
            Assert.Equal("FileName cannot be null or empty (Parameter 'fileName')", exception.Message);
        }
    }
}
