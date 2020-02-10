using System.Collections.Generic;

namespace Client.Models
{
    public class Favorites
    {
        public IEnumerable<string> Movies { get; set; }

        public IEnumerable<string> Foods { get; set; }

        public IEnumerable<string> Music { get; set; }
    }
}