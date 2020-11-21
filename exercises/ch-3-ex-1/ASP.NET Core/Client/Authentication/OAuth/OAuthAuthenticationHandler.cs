using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Client.Extensions;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Client.Authentication.OAuth
{
    public class OAuthAuthenticationHandler : AuthenticationHandler<OAuthAuthenticationOptions>
    {
        public OAuthAuthenticationHandler(IOptionsMonitor<OAuthAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
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