using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Api.Services.Models.Response.Image
{
    public class ImageResponse
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long DateTime { get; set; }
        public string Type { get; set; }
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
        public string AccountUrl { get; set; }
        public long? AccountId { get; set; }
        public bool IsAd { get; set; }
        public bool InMostViral { get; set; }
        public bool HasSound { get; set; }
        public List<TagResponse> Tags { get; set; }
        public int AdType { get; set; }
        public string AdUrl { get; set; }
        public string Edited { get; set; }
        public bool InGallery { get; set; }
        public string Deletehash { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }

        // Video specific
        public string Mp4 { get; set; }
        public long? Mp4Size { get; set; }
        public string Gifv { get; set; }
        public bool? Looping { get; set; }
    }
}
