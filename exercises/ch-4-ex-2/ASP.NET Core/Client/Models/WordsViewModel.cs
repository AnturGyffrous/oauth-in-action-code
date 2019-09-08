using Client.Enums;

namespace Client.Models
{
    public class WordsViewModel
    {
        public WordsResult? Result { get; set; }

        public string Timestamp { get; set; }

        public string Words { get; set; }
    }
}