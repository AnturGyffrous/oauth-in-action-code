using System.Collections.Generic;

namespace Client.Models
{
    public class ProduceViewModel
    {
        public IEnumerable<string> Fruits { get; set; }

        public IEnumerable<string> Meats { get; set; }

        public string Scope { get; set; }

        public IEnumerable<string> Veggies { get; set; }
    }
}