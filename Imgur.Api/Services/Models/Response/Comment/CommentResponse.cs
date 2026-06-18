using Newtonsoft.Json;
using System.Collections.Generic;

namespace Imgur.Api.Services.Models.Response.Comment
{
    public class CommentResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("image_id")]
        public string ImageId { get; set; }

        [JsonProperty("comment")]
        public string Body { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("author_id")]
        public long AuthorId { get; set; }

        [JsonProperty("on_album")]
        public bool OnAlbum { get; set; }

        [JsonProperty("album_cover")]
        public string AlbumCover { get; set; }

        [JsonProperty("ups")]
        public int Ups { get; set; }

        [JsonProperty("downs")]
        public int Downs { get; set; }

        [JsonProperty("points")]
        public int Points { get; set; }

        [JsonProperty("datetime")]
        public long DateTime { get; set; }

        [JsonProperty("parent_id")]
        public long ParentId { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("vote")]
        public string Vote { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("children")]
        public List<CommentResponse> Children { get; set; }
    }
}

