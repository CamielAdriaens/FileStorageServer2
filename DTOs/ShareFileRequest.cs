namespace DTOs
{
    public class ShareFileRequest
    {
        public string FileName { get; set; }
        
        public string MongoFileId { get; set; }
        public string RecipientEmail { get; set; }
    }
}
