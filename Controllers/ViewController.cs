using Microsoft.AspNetCore.Mvc;

namespace GameAssetStorage.Controllers
{
    public class ViewController : Controller
    {
        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();  // This should map to Login.cshtml
        }
    }
}
