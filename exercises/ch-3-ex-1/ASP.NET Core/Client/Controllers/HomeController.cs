using System.Net.Http;
using System.Threading.Tasks;

using Client.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Authorize() => RedirectToAction("Index");

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> FetchResource()
        {
            var client = _httpClientFactory.CreateClient("ProtectedResource");
            var response = await client.PostAsync("resource", null);
            response.EnsureSuccessStatusCode();

            return View(model: await response.Content.ReadAsStringAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return View(new HomeViewModel { AccessToken = accessToken });
        }
    }
}