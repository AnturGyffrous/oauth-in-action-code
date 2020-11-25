using System;

using Microsoft.AspNetCore.Authentication;

namespace Client.Authentication.OAuth
{
    public class OAuthAuthenticationOptions : RemoteAuthenticationOptions
    {
        public OAuthAuthenticationOptions()
        {
            Events = new OAuthAuthenticationEvents();
        }

        public Uri AuthorizationEndpoint { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public new OAuthAuthenticationEvents Events
        {
            get => (OAuthAuthenticationEvents)base.Events;
            set => base.Events = value;
        }

        public string ResponseType { get; set; }

        public Uri TokenEndpoint { get; set; }
    }
}