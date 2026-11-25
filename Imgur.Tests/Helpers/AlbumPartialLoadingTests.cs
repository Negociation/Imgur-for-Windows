using System.Collections.ObjectModel;
using Imgur.Helpers;
using Imgur.Models;
using Xunit;

namespace Imgur.Tests.Helpers
{
    public class AlbumPartialLoadingTests
    {
        [Fact]
        public void Null_media_is_not_partial()
        {
            Assert.False(AlbumPartialLoading.IsPartial(null));
        }

        [Fact]
        public void Single_image_is_not_partial()
        {
            Assert.False(AlbumPartialLoading.IsPartial(new Imgur.Models.Media { IsAlbum = false, ImagesCount = 5 }));
        }

        [Fact]
        public void Minimal_album_from_comment_without_elements_does_not_throw()
        {
            // Mesmo caso de CommentOriginPostHelper.CreateMedia: só Id e IsAlbum.
            var media = new Imgur.Models.Media { IsAlbum = true, ImagesCount = null, Elements = null };

            Assert.False(AlbumPartialLoading.IsPartial(media));
        }

        [Fact]
        public void Album_with_known_count_and_no_elements_is_partial()
        {
            var media = new Imgur.Models.Media { IsAlbum = true, ImagesCount = 5, Elements = null };

            Assert.True(AlbumPartialLoading.IsPartial(media));
        }

        [Fact]
        public void Album_with_all_elements_loaded_is_not_partial()
        {
            var media = new Imgur.Models.Media
            {
                IsAlbum = true,
                ImagesCount = 2,
                Elements = new ObservableCollection<Element> { new Element(), new Element() }
            };

            Assert.False(AlbumPartialLoading.IsPartial(media));
        }

        [Fact]
        public void Album_with_fewer_elements_than_images_is_partial()
        {
            var media = new Imgur.Models.Media
            {
                IsAlbum = true,
                ImagesCount = 5,
                Elements = new ObservableCollection<Element> { new Element() }
            };

            Assert.True(AlbumPartialLoading.IsPartial(media));
        }
    }
}
