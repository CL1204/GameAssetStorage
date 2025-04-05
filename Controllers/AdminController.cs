using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameAssetStorage.Data;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/assets")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/assets/pending
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingAssets()
    {
        var pendingAssets = await _context.Assets
            .Where(a => !a.IsApproved)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.Category,
                a.CreatedAt,
                a.UserId,
                Username = _context.Users
                    .Where(u => u.Id.ToString() == a.UserId)
                    .Select(u => u.username)
                    .FirstOrDefault() ?? "Unknown"
            })
            .ToListAsync();

        return Ok(pendingAssets);
    }


    // POST: api/assets/{id}/approve
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound("Asset not found.");

        asset.IsApproved = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Asset approved." });
    }

    // DELETE: api/assets/{id}/reject
    [HttpDelete("{id}/reject")]
    public async Task<IActionResult> RejectAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound("Asset not found.");

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Asset rejected and deleted." });
    }
}
