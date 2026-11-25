using Imgur.Models;

namespace Imgur.Helpers
{
    /// <summary>
    /// Builds a minimal Media identity from an Account (or any) comment for origin-post navigation.
    /// </summary>
    public static class CommentOriginPostHelper
    {
        public static Media CreateMedia(Comment comment)
        {
            if (comment == null || string.IsNullOrWhiteSpace(comment.ImageId))
                return null;

            return new Media
            {
                Id = comment.ImageId,
                IsAlbum = comment.OnAlbum
            };
        }
    }
}
