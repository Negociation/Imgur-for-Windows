using Imgur.Api.Services.Models.Response.Image;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Api.Services.Models.Response
{
    public class GalleryItemResponse
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int datetime { get; set; }
        public string type { get; set; }
        public bool Animated { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long Size { get; set; }
        public long Views { get; set; }
        public long Bandwidth { get; set; }
        public string Vote { get; set; }
        public bool Favorite { get; set; }
        public bool? Nsfw { get; set; }
        public string Section { get; set; }
        public string account_url { get; set; }
        public long? account_id { get; set; }
        public bool IsAd { get; set; }
        public bool in_most_viral { get; set; }
        public List<TagResponse> Tags { get; set; }
        public string Link { get; set; }
        public int? comment_count { get; set; }
        public int? Ups { get; set; }
        public int? Downs { get; set; }
        public int? Points { get; set; }
        public int Score { get; set; }
        public bool is_album { get; set; }

        // Album specific
        public string Cover { get; set; }
        public int? CoverWidth { get; set; }
        public int? CoverHeight { get; set; }
        public string Privacy { get; set; }
        public string Layout { get; set; }
        public int? images_count { get; set; }
        public List<ImageResponse> Images { get; set; }
    }
}
