﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.API.Resources
{


    public class Image{
        public string id { get; set; }
        public object title { get; set; }
        public string description { get; set; }
        public int datetime { get; set; }
        public string type { get; set; }
        public bool animated { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int size { get; set; }
        public int views { get; set; }
        public object bandwidth { get; set; }
        public object vote { get; set; }
        public bool favorite { get; set; }
        public object nsfw { get; set; }
        public object section { get; set; }
        public object account_url { get; set; }
        public object account_id { get; set; }
        public bool is_ad { get; set; }
        public bool in_most_viral { get; set; }
        public bool has_sound { get; set; }
        public List<object> tags { get; set; }
        public int ad_type { get; set; }
        public string ad_url { get; set; }
        public string edited { get; set; }
        public bool in_gallery { get; set; }
        public string link { get; set; }
        public int mp4_size { get; set; }
        public string mp4 { get; set; }
        public string gifv { get; set; }
        public string hls { get; set; }
        public Processing processing { get; set; }
        public object comment_count { get; set; }
        public object favorite_count { get; set; }
        public object ups { get; set; }
        public object downs { get; set; }
        public object points { get; set; }
        public object score { get; set; }
    }
}


