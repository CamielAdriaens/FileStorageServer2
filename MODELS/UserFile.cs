using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODELS
{
    public class UserFile
    {
        [Key] [Required]
        
        public int FileId { get; set; } // Primary Key
        public string MongoFileId { get; set; } // The MongoDB ObjectId as a string (reference to the file in MongoDB)
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }

        // Foreign key to the user who uploaded this file
        public int UserId { get; set; }
        public User User { get; set; } // Navigation property (for the user who uploaded the file)
    }
}
