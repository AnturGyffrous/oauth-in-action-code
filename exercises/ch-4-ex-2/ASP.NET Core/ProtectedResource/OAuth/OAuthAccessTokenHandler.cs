using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ProtectedResource.Database;

namespace ProtectedResource.OAuth
{
    public class OAuthAccessTokenHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly INoSql _db;

        public OAuthAccessTokenHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            INoSql db)
            : base(options, logger, encoder, clock)
        {
            _db = db;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string accessToken = null;
            if (AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out var authenticationHeader) &&
                authenticationHeader.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                accessToken = authenticationHeader.Parameter;
            }
            else if (Request.HasFormContentType && Request.Form.TryGetValue("access_token", out var accessTokenFormValue))
            {
                accessToken = accessTokenFormValue;
            }
            else if (Request.Query.TryGetValue("access_token", out var accessTokenQueryValue))
            {
                accessToken = accessTokenQueryValue;
            }

            if (accessToken == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid access token"));
            }

            var token = _db.FirstOrDefault(x =>
                x.ContainsKey("access_token") && x["access_token"].ToString() == accessToken);

            if (token == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid access token"));
            }

            var identity = new ClaimsIdentity(Enumerable.Empty<Claim>(), Scheme.Name);
            var principle = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principle, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}