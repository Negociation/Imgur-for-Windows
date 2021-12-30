﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.API.Resources { 
    public class Datum{
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int datetime { get; set; }
        public string cover { get; set; }
        public int cover_width { get; set; }
        public int cover_height { get; set; }
        public string account_url { get; set; }
        public int account_id { get; set; }
        public string privacy { get; set; }
        public string layout { get; set; }
        public int views { get; set; }
        public string link { get; set; }
        public int ups { get; set; }
        public int downs { get; set; }
        public int points { get; set; }
        public int score { get; set; }
        public bool is_album { get; set; }
        public object vote { get; set; }
        public bool favorite { get; set; }
        public bool nsfw { get; set; }
        public string section { get; set; }
        public int comment_count { get; set; }
        public int favorite_count { get; set; }
        public object topic { get; set; }
        public int? topic_id { get; set; }
        public int images_count { get; set; }
        public bool in_gallery { get; set; }
        public bool is_ad { get; set; }
        public List<Tag> tags { get; set; }
        public int ad_type { get; set; }
        public string ad_url { get; set; }
        public bool in_most_viral { get; set; }
        public bool include_album_ads { get; set; }
        public List<Image> images { get; set; }
        public AdConfig ad_config { get; set; }
        public string type { get; set; }
        public bool? animated { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public int? size { get; set; }
        public long? bandwidth { get; set; }
        public bool? has_sound { get; set; }
        public int? edited { get; set; }
    }
}

