using Imgur.Api.Services.Models.Response.Image;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Api.Services.Models.Response
{

    public class AlbumResponse
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public long datetime { get; set; }
        public string cover { get; set; }
        public int cover_width { get; set; }
        public int cover_height { get; set; }
        public string account_url { get; set; }
        public long? account_id { get; set; }
        public string privacy { get; set; }
        public string layout { get; set; }
        public int views { get; set; }
        public string link { get; set; }
        public bool favorite { get; set; }
        public bool? nsfw { get; set; }
        public string section { get; set; }
        public int images_count { get; set; }
        public List<ImageResponse> images { get; set; }
    }
}
