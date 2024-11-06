using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace INTERFACES
{
    public interface IFileRepository
    {
        Task<List<GridFSFileInfo>> GetFilesAsync();
        Task<ObjectId> UploadFileAsync(Stream sourceStream, string fileName);
        Task<Stream> DownloadFileAsync(ObjectId fileId);
        Task DeleteFileAsync(ObjectId fileId);
    }
}
