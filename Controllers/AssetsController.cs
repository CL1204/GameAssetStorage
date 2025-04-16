using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameAssetStorage.Controllers
{
    public class AssetsController : Controller
    {
        [HttpGet("/Assets/Explore")]
        [AllowAnonymous] // ✅ allow guests
        public IActionResult Explore()
        {
            return View();
        }
    }
}
