using System.Collections.Generic;

namespace Imgur.Models
{
    public class AccountPostsPage
    {
        public List<Media> Items { get; set; } = new List<Media>();

        /// <summary>
        /// True se a página bruta da API veio cheia (ainda pode haver mais).
        /// </summary>
        public bool HasMore { get; set; }
    }
}
