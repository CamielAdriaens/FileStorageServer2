namespace Models
{
    public class UserFile
    {
        public int Id { get; set; } // SQL Primary Key
        public string MongoFileId { get; set; } // The MongoDB ObjectId as a string (reference to the file in MongoDB)
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }

        // Foreign key to the user who uploaded this file
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
