using Microsoft.AspNetCore.Mvc;

namespace GameAssetStorage.Controllers
{
    public class DashboardController : Controller
    {
        [HttpGet("/dashboard")]
        public IActionResult Index()
        {
            return View("Index"); // maps to Views/Dashboard/Index.cshtml
        }
    }
}
