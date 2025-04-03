namespace GameAssetStorage.Models
{
    public class Asset
    {
        public int Id { get; set; }

        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string ImageUrl { get; set; }

        public int Likes { get; set; } = 0;
        public bool Approved { get; set; } = false;

        public required string UploadedBy { get; set; }  // username or user ID
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
