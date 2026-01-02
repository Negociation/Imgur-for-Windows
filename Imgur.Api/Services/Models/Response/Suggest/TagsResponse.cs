using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Api.Services.Models.Response.Suggest
{
    public class TagsResponse
    {
        public string name { get; set; }
        public string display_name { get; set; }
        public int followers { get; set; }
        public int total_items { get; set; }
        public bool following { get; set; }
        public bool is_whitelisted { get; set; }
        public string background_hash { get; set; }
        public string thumbnail_hash { get; set; }
        public string accent { get; set; }
        public bool background_is_animated { get; set; }
        public bool thumbnail_is_animated { get; set; }
        public bool is_promoted { get; set; }
        public string description { get; set; }
        public string logo_hash { get; set; }
        public string logo_destination_url { get; set; }
        public object description_annotations { get; set; }
    }
}
