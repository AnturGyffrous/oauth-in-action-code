using System;

using Microsoft.AspNetCore.Authentication;

namespace Client.Authentication.OAuth
{
    public class OAuthAuthenticationOptions : AuthenticationSchemeOptions
    {
        public Uri AuthorizationEndpoint { get; set; }
    }
}