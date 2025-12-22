using Imgur.Helpers;

namespace Imgur.Models
{
    public class Element : Observable
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }

        public string Link { get; set; }

        public bool? has_sound { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string MediaTitle { get; set; }

        public string MediaAuthor { get; set; }

    }
}
