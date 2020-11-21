using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public Task<bool> HandleRequestAsync()
        {
            if (Request.Path == new PathString("/callback"))
            {
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
            }

            return Task.FromResult(false);
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
            Task.FromResult(AuthenticateResult.NoResult());

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var authorizeUrl = Options.AuthorizationEndpoint.AddQueryString(new
            {
                response_type = Options.ResponseType,
                client_id = Options.ClientId,
                redirect_uri = Options.RedirectEndpoint.ToString()
            });

            Response.Redirect(authorizeUrl.ToString());
            return Task.CompletedTask;
        }
    }
}