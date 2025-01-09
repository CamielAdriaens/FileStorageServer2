using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODELS
{
    public class PendingFileShare
    {
        [Key]
        public int ShareId { get; set; } // SQL Primary Key
        public string MongoFileId { get; set; } // Foreign key reference to UserFile
        public UserFile File { get; set; }
        public int FileId { get; set; }
        public string FileName { get; set; }
        public int SenderUserId { get; set; } // User who is sharing the file
        public User SenderUser { get; set; }

        public int RecipientUserId { get; set; } // User to whom the file is shared
        public User RecipientUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // When the file was shared
        public bool IsAccepted { get; set; }
    }
}
