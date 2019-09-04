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

        [HttpPost("resource")]
        public object Resource() => new { name = "Protected Resource", description = "This data has been protected by OAuth 2.0" };
    }
}