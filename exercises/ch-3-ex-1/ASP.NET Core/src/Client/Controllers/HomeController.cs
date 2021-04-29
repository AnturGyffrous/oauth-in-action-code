using Client.Models;

using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View(new HomeViewModel { AccessToken = null });
    }
}