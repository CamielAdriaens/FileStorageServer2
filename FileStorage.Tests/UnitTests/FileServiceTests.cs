using Xunit;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using INTERFACES;
using LOGIC;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using System;

namespace FileStorage.Tests.UnitTests
{
    public class FileServiceTests
    {
        private readonly Mock<IFileRepository> _fileRepositoryMock;
        private readonly FileService _fileService;

        public FileServiceTests()
        {
            _fileRepositoryMock = new Mock<IFileRepository>();
            _fileService = new FileService(_fileRepositoryMock.Object);
        }

        [Fact]
        public async Task UploadFileAsync_ShouldReturnObjectId()
        {
            // Arrange
            var fileName = "file.txt";
            var stream = new MemoryStream();
            var expectedId = ObjectId.GenerateNewId();
            _fileRepositoryMock.Setup(repo => repo.UploadFileAsync(stream, fileName)).ReturnsAsync(expectedId);

            // Act
            var result = await _fileService.UploadFileAsync(stream, fileName);

            // Assert
            Assert.Equal(expectedId, result);
            _fileRepositoryMock.Verify(repo => repo.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DownloadFileAsync_ShouldReturnStream()
        {
            // Arrange
            var fileId = ObjectId.GenerateNewId();
            var expectedStream = new MemoryStream();
            _fileRepositoryMock.Setup(repo => repo.DownloadFileAsync(fileId)).ReturnsAsync(expectedStream);

            // Act
            var result = await _fileService.DownloadFileAsync(fileId);

            // Assert
            Assert.Equal(expectedStream, result);
            _fileRepositoryMock.Verify(repo => repo.DownloadFileAsync(fileId), Times.Once);
        }

        [Fact]
        public async Task DeleteFileAsync_ShouldInvokeRepositoryMethod()
        {
            // Arrange
            var fileId = ObjectId.GenerateNewId();
            _fileRepositoryMock.Setup(repo => repo.DeleteFileAsync(fileId)).Returns(Task.CompletedTask);

            // Act
            await _fileService.DeleteFileAsync(fileId);

            // Assert
            _fileRepositoryMock.Verify(repo => repo.DeleteFileAsync(fileId), Times.Once);
        }
       

        // Test for GetFilesAsync when no files exist
        [Fact]
        public async Task GetFilesAsync_ShouldReturnEmptyList_WhenNoFilesExist()
        {
            // Arrange
            var files = new List<GridFSFileInfo>();

            _fileRepositoryMock.Setup(repo => repo.GetFilesAsync()).ReturnsAsync(files);

            // Act
            var result = await _fileService.GetFilesAsync();

            // Assert
            Assert.Empty(result);
            _fileRepositoryMock.Verify(repo => repo.GetFilesAsync(), Times.Once);
        }

        // Helper method to create a mock GridFSFileInfo
        private GridFSFileInfo CreateMockGridFSFileInfo(string fileName, long length)
        {
            var document = new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "filename", fileName },
                { "uploadDate", DateTime.UtcNow },
                { "length", length }
            };

            return new GridFSFileInfo(document);
        }
    }
}
