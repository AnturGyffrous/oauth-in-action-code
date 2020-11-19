using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Client.Authentication.OAuth
{
    public class OAuthAuthenticationHandler : AuthenticationHandler<OAuthAuthenticationOptions>
    {
        public OAuthAuthenticationHandler(IOptionsMonitor<OAuthAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(AuthenticateResult.NoResult());

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var authorizeUrl = BuildUrl(
                Options.AuthorizationEndpoint.ToString(),
                new
                {
                    response_type = Options.ResponseType,
                    client_id = Options.ClientId,
                    redirect_uri = Options.RedirectEndpoint.ToString()
                });

            Response.Redirect(authorizeUrl);
            return Task.CompletedTask;
        }

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