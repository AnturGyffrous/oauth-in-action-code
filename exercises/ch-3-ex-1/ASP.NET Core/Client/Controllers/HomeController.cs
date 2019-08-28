﻿using System;
using System.Linq;
using System.Web;

using Client.Models;

using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private const string AuthorizationEndpoint = "http://localhost:9001/authorize";
        private const string ClientId = "oauth-client-1";

        [HttpGet("authorize")]
        public IActionResult Authorize()
        {
            var authorizeUrl = BuildUrl(
                AuthorizationEndpoint,
                new
                {
                    response_type = "code",
                    client_id = ClientId
                });

            return Redirect(authorizeUrl);
        }

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