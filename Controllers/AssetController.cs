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

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            return Unauthorized("Invalid user.");

        var userId = userIdClaim.Value;

        try
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(file);
            if (string.IsNullOrEmpty(imageUrl))
                return StatusCode(500, "Cloudinary upload failed.");

            var asset = new Asset
            {
                Title = title,
                Description = description ?? "",
                Category = category.ToLower(),
                ImageUrl = imageUrl,
                FileUrl = imageUrl,
                Tags = tags?.ToArray() ?? Array.Empty<string>(),
                UserId = userId,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asset uploaded and pending approval." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Upload error: " + ex.Message);
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }

    [Authorize]
    [HttpPost("{id}/like")]
    public async Task<IActionResult> LikeAsset(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated.");

        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound("Asset not found.");

        // Check if user already liked
        bool alreadyLiked = await _context.AssetLikes.AnyAsync(al => al.AssetId == id && al.UserId == userId);
        if (alreadyLiked)
            return BadRequest(new { message = "You have already liked this asset." });

        asset.Likes++;

        _context.AssetLikes.Add(new AssetLike
        {
            AssetId = id,
            UserId = userId
        });

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

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound("Asset not found.");

        asset.IsApproved = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Asset approved successfully." });
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}/reject")]
    public async Task<IActionResult> RejectAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound("Asset not found.");

        try
        {
            var success = await _cloudinaryService.DeleteImageAsync(asset.ImageUrl);
            if (!success)
                Console.WriteLine("⚠️ Failed to delete from Cloudinary.");

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asset rejected and deleted." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Reject error: " + ex.Message);
            return StatusCode(500, new { message = "Failed to reject asset", error = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound("Asset not found.");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "true";

        if (asset.UserId != userId && !isAdmin)
        {
            return Forbid();
        }

        try
        {
            var success = await _cloudinaryService.DeleteImageAsync(asset.ImageUrl);
            if (!success)
                Console.WriteLine("⚠️ Failed to delete from Cloudinary.");

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asset deleted successfully." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Delete error: " + ex.Message);
            return StatusCode(500, new { message = "Failed to delete asset", error = ex.Message });
        }
    }


    [Authorize(Policy = "AdminOnly")]
    [HttpGet("pending-assets")]
    public async Task<IActionResult> GetPendingAssets()
    {
        var pendingAssets = await _context.Assets
            .Where(a => !a.IsApproved)
            .OrderByDescending(a => a.CreatedAt)
            .Join(
                _context.Users,
                asset => asset.UserId,
                user => user.Id.ToString(),
                (asset, user) => new
                {
                    asset.Id,
                    asset.Title,
                    asset.Description,
                    asset.Category,
                    asset.ImageUrl,
                    asset.FileUrl,
                    asset.CreatedAt,
                    asset.Tags,
                    asset.Likes,
                    asset.Downloads,
                    username = user.username
                })
            .ToListAsync();

        return Ok(pendingAssets);
    }

    [HttpGet("approved")]
    public async Task<IActionResult> GetApprovedAssets()
    {
        var assets = await _context.Assets
            .Where(a => a.IsApproved)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(assets);
    }

    [HttpGet("top")]
    public async Task<IActionResult> GetTopAssets()
    {
        var topAssets = await _context.Assets
            .Where(a => a.IsApproved)
            .OrderByDescending(a => a.Likes)
            .Take(10)
            .ToListAsync();

        return Ok(topAssets);
    }

    [Authorize]
    [HttpGet("user")]
    public async Task<IActionResult> GetUserAssets()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var assets = await _context.Assets
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(assets);
    }
}
