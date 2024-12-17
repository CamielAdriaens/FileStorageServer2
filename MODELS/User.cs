using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODELS
{
    public class User
    {
        [Key]
        
        public int UserId { get; set; }
        public string GoogleId { get; set; } // Google ID from the token
        public string Email { get; set; }
        public string Name { get; set; }

        // Navigation property for the user's files
        public List<UserFile> UserFiles { get; set; }
    }
}