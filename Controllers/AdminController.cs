using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameAssetStorage.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        [HttpGet("/admin")]
        public IActionResult Panel()
        {
            return View("Panel"); // maps to Views/Admin/Panel.cshtml
        }
    }
}
