using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Client.Models;
using Client.OAuth;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private const string AuthorizationEndpoint = "http://localhost:9001/authorize";
        private const string TokenEndpoint = "http://localhost:9001/token";
        private const string ClientId = "oauth-client-1";
        private const string ClientSecret = "oauth-client-secret-1";
        private const string ClientScope = "foo";
        private const string ProtectedResource = "http://localhost:9002/resource";

        private static readonly string[] RedirectUris = { "http://localhost:9000/callback" };

        private static string _accessToken = "987tghjkiu6trfghjuytrghj";
        private static string _scope;
        private static string _refreshToken = "j2r3oj32r23rmasd98uhjrk2o3i";
        private static string _state;

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly RandomNumberGenerator _prng = RandomNumberGenerator.Create();

        [HttpGet]
        public IActionResult Index()
        {
            return View(new HomeViewModel { AccessToken = _accessToken, RefreshToken = _refreshToken, Scope = _scope });
        }

        [HttpGet("authorize")]
        public IActionResult Authorize()
        {
            _accessToken = null;
            _scope = null;

            var state = new byte[32];
            _prng.GetBytes(state);
            _state = Convert.ToBase64String(state);

            var authorizeUrl = BuildUrl(
                AuthorizationEndpoint,
                new
                {
                    response_type = "code",
                    scope = ClientScope,
                    client_id = ClientId,
                    redirect_uri = RedirectUris[0],
                    state = _state
                });

            return Redirect(authorizeUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string state, string code, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                return View("Error", error);
            }

            if (state != _state)
            {
                return View("Error", "State value did not match");
            }

            var content = new StringContent(
                BuildQueryString(new
                {
                    grant_type = "authorization_code",
                    code,
                    redirect_uri = RedirectUris[0]
                }));

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint) { Content = content };

            var credentials = $"{WebUtility.UrlEncode(ClientId)}:{WebUtility.UrlEncode(ClientSecret)}";
            var authenticationValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationValue);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return View("Error", $"Unable to fetch access token, server response: {response.StatusCode}");
            }

            var tokenResponse =
                JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

            _accessToken = tokenResponse.AccessToken;
            _scope = tokenResponse.Scope;

            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                _refreshToken = tokenResponse.RefreshToken;
            }

            return RedirectToAction("Index");
        }

        [HttpGet("fetch_resource")]
        public async Task<IActionResult> FetchResource()
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                return View("Error", "Missing access token.");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, ProtectedResource);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _accessToken = null;
                if (!string.IsNullOrEmpty(_refreshToken))
                {
                    return await RefreshAccessToken();
                }

                return View("Error", $"Unable to fetch resource. Status {response.StatusCode}");
            }

            var body = await response.Content.ReadAsStringAsync();

            return View("Data", body);
        }

        private async Task<IActionResult> RefreshAccessToken()
        {
            var content = new StringContent(
                BuildQueryString(new
                {
                    grant_type = "refresh_token",
                    refresh_token = _refreshToken
                }));

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint) { Content = content };

            var credentials = $"{WebUtility.UrlEncode(ClientId)}:{WebUtility.UrlEncode(ClientSecret)}";
            var authenticationValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationValue);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return View("Error", $"Unable to fetch access token, server response: {response.StatusCode}");
            }

            var tokenResponse =
                JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

            _accessToken = tokenResponse.AccessToken;
            _scope = tokenResponse.Scope;

            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                _refreshToken = tokenResponse.RefreshToken;
            }

            return RedirectToAction("FetchResource");
        }

        private string BuildUrl(string uri, object queryString = null)
        {
            if (queryString == null)
            {
                return uri;
            }

            var uriBuilder = new UriBuilder(uri) { Query = BuildQueryString(queryString) };

            return uriBuilder.Uri.ToString();
        }

        private string BuildQueryString(object queryString = null)
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
    }
}