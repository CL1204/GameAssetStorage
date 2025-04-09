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
            var username = User.Identity.Name;  // Get the username from the logged-in user's identity
            return View("Me", username);  // Pass the username to the view
        }
    }
}
