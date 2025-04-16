using Microsoft.AspNetCore.Mvc;

namespace GameAssetStorage.Controllers
{
    public class ViewController : Controller
    {
        // This login route is now unique, avoid conflict with AuthController
        [HttpGet("view/login")]
        public IActionResult Login()
        {
            return View();  // Ensure that this maps to a different view, if needed
        }
    }
}
