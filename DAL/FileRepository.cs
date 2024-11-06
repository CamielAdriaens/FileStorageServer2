using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Microsoft.Extensions.Options;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using INTERFACES;


namespace DAL
{
    public class FileRepository : IFileRepository
    {
        private readonly IMongoDatabase _database;
        private readonly IGridFSBucket _gridFsBucket;

        public FileRepository(IOptions<MongoDbSettings> settings)
        {
            if (settings == null || settings.Value == null)
            {
                throw new ArgumentNullException(nameof(settings), "MongoDB settings cannot be null");
            }

            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
            _gridFsBucket = new GridFSBucket(_database);
        }

        public async Task<List<GridFSFileInfo>> GetFilesAsync()
        {
            var filter = Builders<GridFSFileInfo>.Filter.Empty;
            using var cursor = await _gridFsBucket.FindAsync(filter);
            return await cursor.ToListAsync();
        }

        public async Task<ObjectId> UploadFileAsync(Stream sourceStream, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("FileName cannot be null or empty", nameof(fileName));
            }

            return await _gridFsBucket.UploadFromStreamAsync(fileName, sourceStream);
        }

        public async Task<Stream> DownloadFileAsync(ObjectId fileId)
        {
            var destinationStream = new MemoryStream();
            await _gridFsBucket.DownloadToStreamAsync(fileId, destinationStream);
            destinationStream.Position = 0;
            return destinationStream;
        }

        public async Task DeleteFileAsync(ObjectId fileId)
        {
            await _gridFsBucket.DeleteAsync(fileId);
        }
    }
}
