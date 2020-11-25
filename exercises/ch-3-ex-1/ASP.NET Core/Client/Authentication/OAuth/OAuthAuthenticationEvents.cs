using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;

namespace Client.Authentication.OAuth
{
    public class OAuthAuthenticationEvents : RemoteAuthenticationEvents
    {
        public Func<RedirectContext<OAuthAuthenticationOptions>, Task> OnRedirectToAuthorizationEndpoint { get; set; } =
            context =>
            {
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };

        public virtual Task RedirectToAuthorizationEndpoint(RedirectContext<OAuthAuthenticationOptions> context) =>
            OnRedirectToAuthorizationEndpoint(context);
    }
}