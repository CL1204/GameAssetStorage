using System.ComponentModel.DataAnnotations;

namespace GameAssetStorage.Models
{
    public class Asset
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public int Likes { get; set; } = 0;

        public bool IsApproved { get; set; } = false;

        [Required]
        public string UserId { get; set; } = string.Empty;  // or rename to UploadedBy if needed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
