using System;

using Client.Models;

using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("authorize")]
        public IActionResult Authorize() => throw new NotImplementedException();

        [HttpGet]
        public IActionResult Index() => View(new HomeViewModel { AccessToken = null, Scope = null });
    }
}