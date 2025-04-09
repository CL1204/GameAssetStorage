using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameAssetStorage.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        [HttpGet("/profile")]
        public IActionResult Me()
        {
            return View("Me"); // renders Views/Profile/Me.cshtml
        }
    }
}
