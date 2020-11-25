using System.Net.Http;

using Microsoft.Extensions.Options;

namespace Client.Authentication.OAuth
{
    public class OAuthPostConfigureOptions : IPostConfigureOptions<OAuthAuthenticationOptions>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OAuthPostConfigureOptions(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void PostConfigure(string name, OAuthAuthenticationOptions options)
        {
            if (options.Backchannel != null)
            {
                return;
            }

            options.Backchannel = _httpClientFactory.CreateClient();
        }
    }
}