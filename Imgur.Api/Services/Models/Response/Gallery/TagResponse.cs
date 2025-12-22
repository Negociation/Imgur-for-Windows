using Imgur.Api.Services.Models.Response.Image;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Api.Services.Models.Response
{
    public class TagResponse
    {
        public string name { get; set; }
        public string display_name { get; set; }
        public int followers { get; set; }
        public int total_items { get; set; }
        public bool following { get; set; }
        public bool is_whitelisted { get; set; }
        public string background_hash { get; set; }
        public string thumbnail_Hash { get; set; }
        public string accent { get; set; }
        public bool is_promoted { get; set; }
        public string description { get; set; }
        public string logo_hash { get; set; }
        public string logo_destination_url { get; set; }

        public List<GalleryItemResponse> items { get; set; }

    }
}
