using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GameAssetStorage.Services;

[ApiController]
[Route("api/assets")]
public class AssetController : ControllerBase
{
    private readonly CloudinaryService _cloudinaryService;

    public AssetController(CloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
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
    [HttpGet("secure-data")]
    public IActionResult GetSecureData()
    {
        Console.WriteLine("✅ Secure Data endpoint accessed!");
        return Ok(new { message = "You have accessed a protected endpoint!" });
    }
}
