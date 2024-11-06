using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MongoDB.Bson;
using INTERFACES;
using LOGIC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    public class FileServiceTests
    {
        private readonly FileService _fileService;
        private readonly Mock<IFileRepository> _fileRepositoryMock;

        public FileServiceTests()
        {
            _fileRepositoryMock = new Mock<IFileRepository>();
            _fileService = new FileService(_fileRepositoryMock.Object);
        }

        [Fact]
        public async Task UploadFileAsync_ShouldReturnFileId_WhenFileIsUploaded()
        {
            // Arrange
            var fileId = ObjectId.GenerateNewId();
            var stream = new MemoryStream();
            var fileName = "testFile.txt";

            _fileRepositoryMock.Setup(repo => repo.UploadFileAsync(stream, fileName)).ReturnsAsync(fileId);

            // Act
            var result = await _fileService.UploadFileAsync(stream, fileName);

            // Assert
            Assert.Equal(fileId, result);
            _fileRepositoryMock.Verify(repo => repo.UploadFileAsync(stream, fileName), Times.Once);
        }

        [Fact]
        public async Task DownloadFileAsync_ShouldReturnFileStream_WhenFileExists()
        {
            // Arrange
            var fileId = ObjectId.GenerateNewId();
            var memoryStream = new MemoryStream();
            _fileRepositoryMock.Setup(repo => repo.DownloadFileAsync(fileId)).ReturnsAsync(memoryStream);

            // Act
            var result = await _fileService.DownloadFileAsync(fileId);

            // Assert
            Assert.Equal(memoryStream, result);
        }

        [Fact]
        public async Task DeleteFileAsync_ShouldDeleteFile_WhenFileExists()
        {
            // Arrange
            var fileId = ObjectId.GenerateNewId();
            _fileRepositoryMock.Setup(repo => repo.DeleteFileAsync(fileId)).Returns(Task.CompletedTask);

            // Act
            await _fileService.DeleteFileAsync(fileId);

            // Assert
            _fileRepositoryMock.Verify(repo => repo.DeleteFileAsync(fileId), Times.Once);
        }
    }
}