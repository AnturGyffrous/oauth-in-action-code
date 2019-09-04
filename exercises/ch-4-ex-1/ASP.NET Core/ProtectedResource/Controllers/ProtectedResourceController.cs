using Microsoft.AspNetCore.Mvc;

namespace ProtectedResource.Controllers
{
    [ApiController]
    public class ProtectedResourceController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() => View();
    }
}