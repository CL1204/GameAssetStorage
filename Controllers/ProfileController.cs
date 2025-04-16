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
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "AuthView");

            return View("Me", username);
        }
    }
}
