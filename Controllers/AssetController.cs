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
    private readonly S3Service _s3Service;
    private readonly AppDbContext _context;

    public AssetController(S3Service s3Service, AppDbContext context)
    {
        _s3Service = s3Service;
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

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Invalid user.");

        try
        {
            var uploadedUrl = await _s3Service.UploadFileAsync(file);

            var asset = new Asset
            {
                Title = title,
                Description = description ?? "",
                Category = category.ToLower(),
                ImageUrl = uploadedUrl,
                FileUrl = uploadedUrl,
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
    public async Task<IActionResult> ToggleLike(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();

        var existingLike = await _context.AssetLikes.FirstOrDefaultAsync(l => l.AssetId == id && l.UserId == userId);
        if (existingLike != null)
        {
            _context.AssetLikes.Remove(existingLike);
            asset.Likes = Math.Max(0, asset.Likes - 1);
        }
        else
        {
            _context.AssetLikes.Add(new AssetLike { AssetId = id, UserId = userId });
            asset.Likes++;
        }

        await _context.SaveChangesAsync();
        return Ok(new { likes = asset.Likes });
    }

    [Authorize]
    [HttpGet("liked")]
    public async Task<IActionResult> GetLikedAssets()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var likedAssetIds = await _context.AssetLikes
            .Where(like => like.UserId == userId)
            .Select(like => like.AssetId)
            .ToListAsync();

        var assets = await _context.Assets
            .Where(asset => likedAssetIds.Contains(asset.Id) && asset.IsApproved)
            .OrderByDescending(asset => asset.CreatedAt)
            .ToListAsync();

        return Ok(assets);
    }

    [Authorize]
    [HttpPost("{id}/download")]
    public async Task<IActionResult> DownloadAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();

        asset.Downloads++;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Download recorded", downloads = asset.Downloads });
    }

    [HttpGet("{id}/download-file")]
    public async Task<IActionResult> DownloadFile(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound("Asset not found.");

        try
        {
            var fileStream = await _s3Service.GetFileStreamAsync(asset.FileUrl);
            var fileName = Path.GetFileName(new Uri(asset.FileUrl).AbsolutePath);

            return File(fileStream, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ File download error: " + ex.Message);
            return StatusCode(500, "Failed to fetch file from S3.");
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();

        asset.IsApproved = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Asset approved." });
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}/reject")]
    public async Task<IActionResult> RejectAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();

        try
        {
            await _s3Service.DeleteFileAsync(asset.FileUrl);
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asset rejected and deleted." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Reject error: " + ex.Message);
            return StatusCode(500, new { message = "Delete failed", error = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "true";

        if (asset.UserId != userId && !isAdmin)
            return Forbid();

        try
        {
            await _s3Service.DeleteFileAsync(asset.FileUrl);
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asset deleted." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Delete error: " + ex.Message);
            return StatusCode(500, new { message = "Delete failed", error = ex.Message });
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("pending-assets")]
    public async Task<IActionResult> GetPendingAssets()
    {
        var assets = await (from asset in _context.Assets
                            join user in _context.Users on asset.UserId equals user.Id.ToString()
                            where !asset.IsApproved
                            orderby asset.CreatedAt descending
                            select new
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
                            }).ToListAsync();

        return Ok(assets);
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

    [Authorize]
    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return BadRequest("Comment cannot be empty.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var asset = await _context.Assets.FindAsync(id);
        if (asset == null || !asset.IsApproved) return NotFound("Asset not found or not approved.");

        var comment = new AssetComment
        {
            AssetId = id,
            UserId = int.Parse(userId),
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        _context.AssetComments.Add(comment);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Comment added." });
    }

    [HttpGet("{id}/comments")]
    public async Task<IActionResult> GetComments(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null || !asset.IsApproved) return NotFound("Asset not found or not approved.");

        var comments = await _context.AssetComments
            .Where(c => c.AssetId == id)
            .OrderByDescending(c => c.CreatedAt)
            .Join(_context.Users,
                  comment => comment.UserId,
                  user => user.Id,
                  (comment, user) => new
                  {
                      comment.Id,
                      user.username,
                      comment.Content,
                      comment.CreatedAt
                  })
            .ToListAsync();

        return Ok(comments);
    }

    [Authorize]
    [HttpDelete("comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "true";

        var comment = await _context.AssetComments.FindAsync(commentId);
        if (comment == null)
            return NotFound();

        if (comment.UserId.ToString() != userId && !isAdmin)
            return Forbid();

        _context.AssetComments.Remove(comment);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Comment deleted." });
    }
}