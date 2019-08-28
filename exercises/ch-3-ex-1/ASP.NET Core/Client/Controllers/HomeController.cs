using System;
using System.Linq;
using System.Web;

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

        private static string BuildQueryString(object queryString = null)
        {
            if (queryString == null)
            {
                return null;
            }

            var parameters = HttpUtility.ParseQueryString(string.Empty);

            queryString
                .GetType()
                .GetProperties()
                .ToList()
                .ForEach(x => parameters.Add(x.Name, x.GetValue(queryString, null).ToString()));

            return parameters.ToString();
        }

        private static string BuildUrl(string uri, object queryString = null)
        {
            if (queryString == null)
            {
                return uri;
            }

            var uriBuilder = new UriBuilder(uri) { Query = BuildQueryString(queryString) };

            return uriBuilder.Uri.ToString();
        }
    }
}