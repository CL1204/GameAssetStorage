using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameAssetStorage.Data;
using Microsoft.EntityFrameworkCore;

namespace GameAssetStorage.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminApiController(AppDbContext context)
        {
            _context = context;
        }

        // POST: /api/admin/{id}/approve
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveAsset(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return NotFound("Asset not found.");

            asset.IsApproved = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Asset approved." });
        }

        // DELETE: /api/admin/{id}/reject
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
}
