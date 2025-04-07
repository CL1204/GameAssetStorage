using GameAssetStorage.Models;
using System.ComponentModel.DataAnnotations;

public class AssetLike
{
    public int Id { get; set; }

    public int AssetId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty; // Prevent CS8618 warning

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Asset Asset { get; set; } = null!;
}
