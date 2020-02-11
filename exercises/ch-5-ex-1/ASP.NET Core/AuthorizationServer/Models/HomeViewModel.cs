using System.Collections.Generic;

namespace AuthorizationServer.Models
{
    public class HomeViewModel
    {
        public AuthorizationServer AuthorizationServer { get; set; }

        public IEnumerable<Client> Clients { get; set; }
    }
}