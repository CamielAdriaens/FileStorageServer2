using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODELS
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "GoogleId cannot exceed 50 characters.")]
        public string GoogleId { get; set; } // Google ID from the token

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        // Navigation property for the user's files
        public List<UserFile> UserFiles { get; set; }
    }
}
