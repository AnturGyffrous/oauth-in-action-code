using System;

using Microsoft.AspNetCore.Authentication;

namespace Client.Authentication.OAuth
{
    public class OAuthAuthenticationOptions : RemoteAuthenticationOptions
    {
        public Uri AuthorizationEndpoint { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public Uri RedirectEndpoint { get; set; }

        public string ResponseType { get; set; }

        public Uri TokenEndpoint { get; set; }
    }
}