using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Client.Enums;
using Client.Models;
using Client.OAuth;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private const string AuthorizationEndpoint = "http://localhost:9001/authorize";
        private const string ClientId = "oauth-client-1";
        private const string ClientScope = "read write delete";
        private const string ClientSecret = "oauth-client-secret-1";
        private const string ClientUri = "http://localhost:9000";
        private const string ProtectedResource = "http://localhost:9002/resource";
        private const string TokenEndpoint = "http://localhost:9001/token";
        private const string WordsApi = "http://localhost:9002/words";

        private static string _accessToken;
        private static string _refreshToken;
        private static string _scope;
        private static string _state;

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly RandomNumberGenerator _prng = RandomNumberGenerator.Create();

        [HttpGet("add_word")]
        public async Task<IActionResult> AddWord(string word)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, WordsApi);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            request.Content = new StringContent(BuildQueryString(new { word }), Encoding.UTF8,
                "application/x-www-form-urlencoded");

            var response = await _httpClient.SendAsync(request);

            return View("Words",
                response.IsSuccessStatusCode
                    ? new WordsViewModel { Result = WordsResult.AddSuccess }
                    : new WordsViewModel { Result = WordsResult.AddFailure });
        }

        [HttpGet("authorize")]
        public IActionResult Authorize()
        {
            _accessToken = null;

            var state = new byte[36];
            _prng.GetBytes(state);
            _state = Convert.ToBase64String(state);

            var authorizeUrl = BuildUrl(
                AuthorizationEndpoint,
                new
                {
                    response_type = "code",
                    scope = ClientScope,
                    client_id = ClientId,
                    redirect_uri = ClientUri + Url.Action("Callback"),
                    state = _state
                });

            return Redirect(authorizeUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code, string state, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                return View("Error", error);
            }

            if (string.IsNullOrEmpty(state) || state != _state)
            {
                return View("Error", "State value did not match");
            }

            var response = await PostMessageToTokenEndpoint(new
            {
                grant_type = "authorization_code",
                code,
                redirect_uri = ClientUri + Url.Action("Callback")
            });

            if (!response.IsSuccessStatusCode)
            {
                return View("Error", $"Unable to fetch access token, server response: {response.StatusCode}");
            }

            var tokenResponse = JsonConvert
                .DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

            _accessToken = tokenResponse.AccessToken;
            _scope = tokenResponse.Scope;

            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                _refreshToken = tokenResponse.RefreshToken;
            }

            return RedirectToAction("Index");
        }

        [HttpGet("delete_word")]
        public async Task<IActionResult> DeleteWord(string word)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, WordsApi);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _httpClient.SendAsync(request);

            return View("Words",
                response.IsSuccessStatusCode
                    ? new WordsViewModel { Result = WordsResult.RemoveSuccess }
                    : new WordsViewModel { Result = WordsResult.RemoveFailure });
        }

        [HttpGet("fetch_resource")]
        public async Task<IActionResult> FetchResource()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, ProtectedResource);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _accessToken = null;
                if (!string.IsNullOrEmpty(_refreshToken))
                {
                    return await RefreshAccessToken();
                }

                return RedirectToAction("Authorize");
            }

            if (!response.IsSuccessStatusCode)
            {
                return View("Error", $"Server returned response code: {response.StatusCode}");
            }

            var body = await response.Content.ReadAsStringAsync();

            return View("Data", JToken.Parse(body).ToString(Formatting.Indented));
        }

        [HttpGet("get_words")]
        public async Task<IActionResult> GetWords()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, WordsApi);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return View("Words", new WordsViewModel { Result = WordsResult.GetFailure });
            }

            var wordsResponse =
                JsonConvert.DeserializeObject<WordsResponse>(await response.Content.ReadAsStringAsync());

            return View("Words",
                new WordsViewModel
                {
                    Words = wordsResponse.Words, Timestamp = wordsResponse.Timestamp,
                    Result = WordsResult.GetSuccess
                });
        }

        [HttpGet]
        public IActionResult Index() => View(
            new HomeViewModel
            {
                AccessToken = _accessToken,
                Scope = _scope,
                RefreshToken = _refreshToken
            });

        [HttpGet("words")]
        public IActionResult Words() => View();

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

        private Task<HttpResponseMessage> PostMessageToTokenEndpoint(object request)
        {
            var content = new StringContent(BuildQueryString(request));

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint) { Content = content };

            var credentials = $"{WebUtility.UrlEncode(ClientId)}:{WebUtility.UrlEncode(ClientSecret)}";
            var authenticationValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationValue);

            return _httpClient.SendAsync(requestMessage);
        }

        private async Task<IActionResult> RefreshAccessToken()
        {
            var response = await PostMessageToTokenEndpoint(new
            {
                grant_type = "refresh_token",
                refresh_token = _refreshToken
            });

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _accessToken = null;
                _refreshToken = null;
                return RedirectToAction("Authorize");
            }

            if (!response.IsSuccessStatusCode)
            {
                _refreshToken = null;
                return View("Error", $"Unable to fetch access token, server response: {response.StatusCode}");
            }

            var tokenResponse = JsonConvert
                .DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

            _accessToken = tokenResponse.AccessToken;
            _refreshToken = tokenResponse.RefreshToken;

            return RedirectToAction("FetchResource");
        }
    }
}