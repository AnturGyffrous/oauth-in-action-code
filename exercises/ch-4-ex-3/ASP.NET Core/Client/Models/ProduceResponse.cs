using System.Collections.Generic;

namespace Client.Models
{
    public class ProduceResponse
    {
        public IEnumerable<string> Fruit { get; set; }

        public IEnumerable<string> Meats { get; set; }

        public IEnumerable<string> Veggies { get; set; }
    }
}