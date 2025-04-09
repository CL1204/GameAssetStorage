using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameAssetStorage.Controllers
{
    [Authorize]
    public class AssetsController : Controller
    {
        [HttpGet("/assets")]
        public IActionResult Explore()
        {
            return View("Explore"); // maps to Views/Assets/Explore.cshtml
        }
    }
}
