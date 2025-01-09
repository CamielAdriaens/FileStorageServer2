using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODELS
{
    public class UserFile
    {
        [Key]
        [Required]
        public int FileId { get; set; } // Primary Key

        [Required]
        [StringLength(50, ErrorMessage = "MongoFileId cannot exceed 50 characters.")]
        public string MongoFileId { get; set; } // The MongoDB ObjectId as a string

        [Required]
        [StringLength(255, ErrorMessage = "FileName cannot exceed 255 characters.")]
        public string FileName { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; } // Foreign key

        public User User { get; set; } // Navigation property (for the user who uploaded the file)
    }
}
