using Microsoft.AspNetCore.Mvc;

namespace GameAssetStorage.Controllers
{
    public class AuthViewController : Controller
    {
        [HttpGet("/register")]
        public IActionResult Register()
        {
            return View("~/Views/Auth/Register.cshtml");
        }

        [HttpGet("/login")]
        public IActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml");
        }
    }
}
