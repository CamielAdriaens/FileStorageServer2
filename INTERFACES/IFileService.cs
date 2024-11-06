using MongoDB.Bson;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace INTERFACES
{
    public interface IFileService
    {
        Task<List<object>> GetFilesAsync();
        Task<ObjectId> UploadFileAsync(Stream sourceStream, string fileName);
        Task<Stream> DownloadFileAsync(ObjectId fileId);
        Task DeleteFileAsync(ObjectId fileId);
    }
}
