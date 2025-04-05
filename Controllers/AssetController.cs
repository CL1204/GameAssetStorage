using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GameAssetStorage.Services;
using GameAssetStorage.Data;
using GameAssetStorage.Models;
using Microsoft.EntityFrameworkCore;

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

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var fileUrl = await _cloudinaryService.UploadImageAsync(file);

        return Ok(new { Url = fileUrl });
    }

    [Authorize]
    [HttpPost("{id}/like")]
    public async Task<IActionResult> LikeAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound("Asset not found.");

        asset.Likes++; // ✅ Fixed casing
        await _context.SaveChangesAsync();
        return Ok(new { message = "Liked!", likes = asset.Likes });
    }

    [Authorize]
    [HttpPost("{id}/download")]
    public async Task<IActionResult> DownloadAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound("Asset not found.");

        asset.Downloads++; // ✅ Fixed casing
        await _context.SaveChangesAsync();
        return Ok(new { message = "Download recorded", downloads = asset.Downloads });
    }

    [Authorize]
    [HttpGet("secure-data")]
    public IActionResult GetSecureData()
    {
        Console.WriteLine("✅ Secure Data endpoint accessed!");
        return Ok(new { message = "You have accessed a protected endpoint!" });
    }
}
