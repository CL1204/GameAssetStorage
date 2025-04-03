using Microsoft.AspNetCore.Mvc;

namespace GameAssetStorage.Controllers
{
    [ApiController]
    [Route("/")]
    public class RootController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            Console.WriteLine("Health check hit"); // 🟡 Add this line
            return Ok("Backend is running.");
        }
    }
}
