using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Required]
        public string FileUrl { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        // ✅ Use PostgreSQL array mapping
        [Column(TypeName = "text[]")]
        public string[] Tags { get; set; } = Array.Empty<string>();

        public int Likes { get; set; } = 0;

        public int Downloads { get; set; } = 0;

        public bool IsApproved { get; set; } = false;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
