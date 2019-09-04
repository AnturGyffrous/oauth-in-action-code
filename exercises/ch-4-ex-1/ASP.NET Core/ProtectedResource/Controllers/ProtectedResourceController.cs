using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProtectedResource.Controllers
{
    [ApiController]
    [Authorize]
    public class ProtectedResourceController : Controller
    {
        [AllowAnonymous]
        [HttpGet("")]
        public IActionResult Index() => View();
    }
}