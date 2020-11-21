using System.Runtime.Serialization;

namespace Client.Authentication.OAuth
{
    [DataContract]
    public class TokenResponse
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }
    }
}