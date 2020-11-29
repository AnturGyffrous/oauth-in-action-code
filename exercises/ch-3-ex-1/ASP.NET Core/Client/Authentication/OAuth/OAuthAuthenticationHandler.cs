using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Client.Extensions;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Client.Authentication.OAuth
{
    public class OAuthAuthenticationHandler : AuthenticationHandler<OAuthAuthenticationOptions>,
        IAuthenticationRequestHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RandomNumberGenerator _prng = RandomNumberGenerator.Create();

        public OAuthAuthenticationHandler(
            IOptionsMonitor<OAuthAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IHttpClientFactory httpClientFactory)
            : base(options, logger, encoder, clock)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> HandleRequestAsync()
        {
            if (Request.Path == new PathString("/callback"))
            {
                var state = Request.Query["state"];

                if (string.IsNullOrEmpty(state) || state != Context.Session.GetString("State"))
                {
                    throw new InvalidOperationException();
                }

                var code = Request.Query["code"];
                var content = new StringContent(new
                {
                    grant_type = "authorization_code",
                    code,
                    redirect_uri = Options.RedirectEndpoint.ToString()
                }.AsQueryString());

                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var request = new HttpRequestMessage(HttpMethod.Post, Options.TokenEndpoint) { Content = content };

                var credentials = $"{WebUtility.UrlEncode(Options.ClientId)}:{WebUtility.UrlEncode(Options.ClientSecret)}";
                var authenticationValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));

                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationValue);

                var response = await _httpClientFactory.CreateClient().SendAsync(request);

                response.EnsureSuccessStatusCode();

                var tokenResponse = await response.Content.ReadAsAsync<TokenResponse>();

                Context.Session.SetString("AccessToken", tokenResponse.AccessToken);
                Context.Session.SetString("TokenType", tokenResponse.TokenType);

                Response.Redirect(Context.Session.GetString("RequestPath") ?? "/");

                return true;
            }

            return false;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var accessToken = Context.Session.GetString("AccessToken");
            if (accessToken != null)
            {
                var identity = new ClaimsIdentity(Enumerable.Empty<Claim>(), Scheme.Name);
                var principle = new ClaimsPrincipal(identity);
                var properties = new AuthenticationProperties();
                properties.StoreTokens(new[] { new AuthenticationToken { Name = "access_token", Value = accessToken } });
                var ticket = new AuthenticationTicket(principle, properties, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var state = GenerateRandomState();

            Context.Session.SetString("RequestPath", Request.Path);
            Context.Session.SetString("State", state);

            var authorizeUrl = Options.AuthorizationEndpoint.AddQueryString(new
            {
                response_type = Options.ResponseType,
                client_id = Options.ClientId,
                redirect_uri = Options.RedirectEndpoint.ToString(),
                state
            });

            Response.Redirect(authorizeUrl.ToString());
            return Task.CompletedTask;
        }

        private string GenerateRandomState()
        {
            var state = new byte[36];
            _prng.GetBytes(state);
            return Convert.ToBase64String(state);
        }
    }
}