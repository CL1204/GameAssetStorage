using Microsoft.AspNetCore.Mvc;

namespace GameAssetStorage.Controllers
{
    public class RootController : Controller
    {
        [HttpGet("/")]
        public IActionResult Index()
        {
            return Redirect("/dashboard");
        }
    }
}
