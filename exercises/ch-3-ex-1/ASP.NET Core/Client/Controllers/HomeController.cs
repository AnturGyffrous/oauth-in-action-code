using Client.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Authorize() => RedirectToAction("Index");

        public IActionResult Index() => View(new HomeViewModel { AccessToken = null });
    }
}