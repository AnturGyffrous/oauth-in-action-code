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
    public class OAuthAuthenticationHandler : RemoteAuthenticationHandler<OAuthAuthenticationOptions>
    {
        private readonly RandomNumberGenerator _prng = RandomNumberGenerator.Create();

        public OAuthAuthenticationHandler(
            IOptionsMonitor<OAuthAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected new OAuthAuthenticationEvents Events
        {
            get => (OAuthAuthenticationEvents)base.Events;
            set => base.Events = value;
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (string.IsNullOrEmpty(properties.RedirectUri))
            {
                properties.RedirectUri = OriginalPathBase + OriginalPath + Request.QueryString;
            }

            var state = GenerateRandomState();

            Context.Session.SetString("RequestPath", properties.RedirectUri);
            Context.Session.SetString("State", state);

            var authorizeUrl = Options.AuthorizationEndpoint.AddQueryString(new
            {
                response_type = Options.ResponseType,
                client_id = Options.ClientId,
                redirect_uri = BuildRedirectUri(Options.CallbackPath),
                state
            });

            var redirectContext =
                new RedirectContext<OAuthAuthenticationOptions>(Context, Scheme, Options, properties,
                    authorizeUrl.ToString());

            await Events.RedirectToAuthorizationEndpoint(redirectContext);
        }

        protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            var state = Request.Query["state"];

            if (string.IsNullOrEmpty(state) || state != Context.Session.GetString("State"))
            {
                return HandleRequestResult.Fail("State value did not match");
            }

            var code = Request.Query["code"];
            var content = new StringContent(new
            {
                grant_type = "authorization_code",
                code,
                redirect_uri = BuildRedirectUri(Options.CallbackPath)
            }.AsQueryString());

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var request = new HttpRequestMessage(HttpMethod.Post, Options.TokenEndpoint) { Content = content };

            var credentials = $"{WebUtility.UrlEncode(Options.ClientId)}:{WebUtility.UrlEncode(Options.ClientSecret)}";
            var authenticationValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationValue);

            var response = await Options.Backchannel.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return HandleRequestResult.Fail(
                    $"Unable to fetch access token, server response: {response.StatusCode}");
            }

            var tokenResponse = await response.Content.ReadAsAsync<TokenResponse>();
            var identity = new ClaimsIdentity(Enumerable.Empty<Claim>(), Scheme.Name);
            var principle = new ClaimsPrincipal(identity);
            var properties = new AuthenticationProperties { RedirectUri = Context.Session.GetString("RequestPath") };
            properties.StoreTokens(new[]
                { new AuthenticationToken { Name = "access_token", Value = tokenResponse.AccessToken } });
            var ticket = new AuthenticationTicket(principle, properties, Scheme.Name);
            return HandleRequestResult.Success(ticket);
        }

        private string GenerateRandomState()
        {
            var state = new byte[36];
            _prng.GetBytes(state);
            return Convert.ToBase64String(state);
        }
    }
}