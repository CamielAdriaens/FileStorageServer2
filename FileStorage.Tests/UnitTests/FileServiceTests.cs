using Xunit;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using INTERFACES;
using LOGIC;
using MongoDB.Driver.GridFS;
using System.Reflection;
using System.Runtime.Serialization;

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
            var fileName = "file.txt";
            var stream = new MemoryStream();
            var expectedId = MongoDB.Bson.ObjectId.GenerateNewId();
            _fileRepositoryMock.Setup(repo => repo.UploadFileAsync(stream, fileName)).ReturnsAsync(expectedId);

            var result = await _fileService.UploadFileAsync(stream, fileName);

            Assert.Equal(expectedId, result);
        }

        [Fact]
        public async Task DownloadFileAsync_ShouldReturnStream()
        {
            var fileId = MongoDB.Bson.ObjectId.GenerateNewId();
            var expectedStream = new MemoryStream();
            _fileRepositoryMock.Setup(repo => repo.DownloadFileAsync(fileId)).ReturnsAsync(expectedStream);

            var result = await _fileService.DownloadFileAsync(fileId);

            Assert.Equal(expectedStream, result);
        }

        [Fact]
        public async Task DeleteFileAsync_ShouldInvokeRepositoryMethod()
        {
            var fileId = MongoDB.Bson.ObjectId.GenerateNewId();
            _fileRepositoryMock.Setup(repo => repo.DeleteFileAsync(fileId)).Returns(Task.CompletedTask);

            await _fileService.DeleteFileAsync(fileId);

            _fileRepositoryMock.Verify(repo => repo.DeleteFileAsync(fileId), Times.Once);
        }
    }
}