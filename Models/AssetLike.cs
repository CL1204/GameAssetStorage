using GameAssetStorage.Models;

public class AssetLike
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string UserId { get; set; } = string.Empty;

    public Asset? Asset { get; set; }
}
