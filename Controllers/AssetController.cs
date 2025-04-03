using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using GameAssetStorage.Services;

[ApiController]
[Route("api/assets")]
public class AssetController : ControllerBase
{
    private readonly S3Service _s3Service;

    public AssetController(S3Service s3Service)
    {
        _s3Service = s3Service;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        using var stream = file.OpenReadStream();
        var fileUrl = await _s3Service.UploadFileAsync(file.FileName, stream);

        return Ok(new { Url = fileUrl });
    }

    // ✅ Protected Endpoint: Requires JWT Authentication
    [Authorize]
    [HttpGet("secure-data")]
    public IActionResult GetSecureData()
    {
        Console.WriteLine("✅ Secure Data endpoint accessed!");  // 🔹 Added log for debugging
        return Ok(new { message = "You have accessed a protected endpoint!" });
    }
}
