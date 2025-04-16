using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameAssetStorage.Controllers
{
    public class DashboardController : Controller
    {
        [HttpGet("/Dashboard")]
        [AllowAnonymous] // ✅ allow guests
        public IActionResult Index()
        {
            return View();
        }
    }
}
