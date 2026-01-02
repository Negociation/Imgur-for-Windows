using Imgur.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Imgur.Models
{
    public class Media : Observable
    {
        public bool IsAlbum { get; set; }

        public int? ImagesCount { get; set; }
        public string CoverType { get; set; }

        public string CoverLink { get; set; }
        public string CoverUri { get; set; }


        public string CoverVideo { get; set; }
        public string CoverImage { get; set; }
        public string CoverPlaceholder { get; set; }

        public bool CoverLoaded { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }

        public long DateTime { get; set; }   // timestamp em segundos
        public long Views { get; set; }
        public int? Ups { get; set; }
        public int? Downs { get; set; }
        public int? Votes { get; set; }
        public long Likes { get; set; }
        public long? CommentCount { get; set; }

        public string AccountId { get; set; }
        public bool MostViral { get; set; }

        public string Embed
        {
            get
            {
                return "<blockquote class=\"imgur - embed - pub\" lang=\"en\" data-id=\"a / " + Id + "\"  ><a href=\"//imgur.com/a/" + Id + "\">" + Title + "</a></blockquote><script async src=\"//s.imgur.com/min/embed.js\" charset=\"utf-8\"></script>";
            }
        }

        public ObservableCollection<Element> Elements { get; set; }
    }
}
