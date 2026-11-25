using Imgur.Api.Services.Models.Response.Comment;
using Imgur.Helpers;
using Imgur.Mappers;
using System.Collections.Generic;
using Xunit;

namespace Imgur.Tests.Mappers
{
    public class CommentMapperTests
    {
        [Fact]
        public void Maps_OnAlbum_and_builds_origin_media_type()
        {
            var mapper = new CommentMapper();
            var comment = mapper.ToComment(new CommentResponse
            {
                Id = 1,
                ImageId = "abc123",
                OnAlbum = true,
                Body = "hello",
                Children = new List<CommentResponse>()
            });

            Assert.True(comment.OnAlbum);
            Assert.Equal("abc123", comment.ImageId);

            var media = CommentOriginPostHelper.CreateMedia(comment);
            Assert.Equal("abc123", media.Id);
            Assert.True(media.IsAlbum);
        }

        [Fact]
        public void CreateMedia_returns_null_without_image_id()
        {
            Assert.Null(CommentOriginPostHelper.CreateMedia(null));
            Assert.Null(CommentOriginPostHelper.CreateMedia(new Imgur.Models.Comment { ImageId = "  " }));
        }

        [Fact]
        public void Maps_children_when_present()
        {
            var mapper = new CommentMapper();
            var comment = mapper.ToComment(new CommentResponse
            {
                Id = 10,
                ImageId = "xyz",
                OnAlbum = false,
                Body = "parent",
                Children = new List<CommentResponse>
                {
                    new CommentResponse { Id = 11, ImageId = "xyz", Body = "child", ParentId = 10 }
                }
            });

            Assert.False(comment.OnAlbum);
            Assert.True(comment.HasReplies);
            Assert.Equal(1, comment.ReplyCount);
            Assert.Equal(11, comment.Children[0].Id);

            var media = CommentOriginPostHelper.CreateMedia(comment);
            Assert.Equal("xyz", media.Id);
            Assert.False(media.IsAlbum);
        }
    }
}
