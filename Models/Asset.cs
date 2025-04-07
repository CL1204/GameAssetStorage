using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameAssetStorage.Models
{
    public class Asset
    {
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column("imageurl")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [Column("fileurl")]
        public string FileUrl { get; set; } = string.Empty;

        [Required]
        [Column("category")]
        public string Category { get; set; } = string.Empty;

        [Column("tags", TypeName = "text[]")]
        public string[] Tags { get; set; } = Array.Empty<string>();

        [Column("likes")]
        public int Likes { get; set; } = 0;

        [Column("downloads")]
        public int Downloads { get; set; } = 0;

        [Column("isapproved")]
        public bool IsApproved { get; set; } = false;

        [Required]
        [Column("userid")]
        public string UserId { get; set; } = string.Empty;

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
