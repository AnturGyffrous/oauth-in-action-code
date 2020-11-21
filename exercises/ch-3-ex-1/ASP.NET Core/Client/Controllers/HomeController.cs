using Client.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        [HttpGet]
        public IActionResult Authorize() => RedirectToAction("Index");

        [HttpGet]
        public IActionResult Index() => View(new HomeViewModel { AccessToken = null });
    }
}