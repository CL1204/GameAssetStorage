using Microsoft.AspNetCore.Mvc;

namespace GameAssetStorage.Controllers
{
    public class AuthViewController : Controller
    {
        [HttpGet("/login")]
        public IActionResult Login() => View("~/Views/Auth/Login.cshtml");

        [HttpGet("/register")]
        public IActionResult Register() => View("~/Views/Auth/Register.cshtml");
    }
}
