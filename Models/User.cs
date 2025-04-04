using System.ComponentModel.DataAnnotations;

namespace GameAssetStorage.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string username { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;

        public bool is_admin { get; set; } = false;

        public bool is_banned { get; set; } = false; // ✅ Fixes CS1061 error
    }
}
