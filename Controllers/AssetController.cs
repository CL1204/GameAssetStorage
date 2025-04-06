using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameAssetStorage.Services;
using GameAssetStorage.Data;
using GameAssetStorage.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/assets")]
public class AssetController : ControllerBase
{
    private readonly CloudinaryService _cloudinaryService;
    private readonly AppDbContext _context;

    public AssetController(CloudinaryService cloudinaryService, AppDbContext context)
    {
        _cloudinaryService = cloudinaryService;
        _context = context;
    }

    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadAsset(
        [FromForm] IFormFile file,
        [FromForm] string title,
        [FromForm] string description,
        [FromForm] string category,
        [FromForm] List<string> tags)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(category))
            return BadRequest("Title and category are required.");

        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Invalid user.");

        try
        {
            string imageUrl = await _cloudinaryService.UploadImageAsync(file);

            var asset = new Asset
            {
                Title = title,
                Description = description,
                Category = category.ToLower(),
                ImageUrl = imageUrl,
                FileUrl = imageUrl,
                Tags = tags.ToArray(), // ✅ Store as string[]
                UserId = userId,
                IsApproved = false
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asset uploaded and pending approval." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Upload error: " + ex.Message);
            return StatusCode(500, "Server error during upload.");
        }
    }

    [Authorize]
    [HttpPost("{id}/like")]
    public async Task<IActionResult> LikeAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound("Asset not found.");

        asset.Likes++;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Liked!", likes = asset.Likes });
    }

    [Authorize]
    [HttpPost("{id}/download")]
    public async Task<IActionResult> DownloadAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound("Asset not found.");

        asset.Downloads++;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Download recorded", downloads = asset.Downloads });
    }
}
