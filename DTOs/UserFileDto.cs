namespace DTOs
{
    public class UserFileDTO
    {
        public int FileId { get; set; } // SQL Primary Key
        public string MongoFileId { get; set; } // The MongoDB ObjectId as a string (reference to the file in MongoDB)
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
    }
}