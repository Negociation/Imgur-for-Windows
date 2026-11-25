using Imgur.Api.Services.Models.Enum;
using Imgur.Helpers;
using Xunit;

namespace Imgur.Tests.Helpers
{
    public class AvatarSeedResolverTests
    {
        private const string SessionAvatar = "https://imgur.com/user/mtiago/avatar";
        private const string ApiAvatar = "https://i.imgur.com/real-photo.png";

        [Fact]
        public void Prefers_session_avatar_over_api_url_so_preset_cdn_does_not_flash_over_real_photo()
        {
            var seed = AvatarSeedResolver.Resolve(SessionAvatar, "https://i.imgur.com/default-preset.png", "default");

            Assert.Equal(SessionAvatar, seed.DisplayUrl);
            Assert.True(seed.HasPreset);
            Assert.Equal(ImgurAvatar.Default, seed.Preset);
            Assert.Equal("default/default", seed.PresetApiValue);
        }

        [Fact]
        public void Falls_back_to_api_url_when_session_avatar_is_missing()
        {
            var seed = AvatarSeedResolver.Resolve(null, ApiAvatar, "taco");

            Assert.Equal(ApiAvatar, seed.DisplayUrl);
            Assert.Equal(ImgurAvatar.Taco, seed.Preset);
        }

        [Fact]
        public void Keeps_session_avatar_when_api_url_is_missing()
        {
            var seed = AvatarSeedResolver.Resolve(SessionAvatar, null, "taco");

            Assert.Equal(SessionAvatar, seed.DisplayUrl);
            Assert.Equal(ImgurAvatar.Taco, seed.Preset);
        }

        [Fact]
        public void Uses_preset_image_only_when_no_real_url_is_known()
        {
            var seed = AvatarSeedResolver.Resolve("", " ", "taco");

            Assert.Equal("ms-appx:///Assets/Avatars/taco.png", seed.DisplayUrl);
        }

        [Fact]
        public void Has_no_preset_for_unknown_avatar_name()
        {
            var seed = AvatarSeedResolver.Resolve(SessionAvatar, ApiAvatar, "custom-upload-123");

            Assert.Equal(SessionAvatar, seed.DisplayUrl);
            Assert.False(seed.HasPreset);
            Assert.Equal(string.Empty, seed.PresetApiValue);
        }

        [Fact]
        public void Returns_empty_display_url_when_nothing_is_known()
        {
            var seed = AvatarSeedResolver.Resolve(null, null, null);

            Assert.Equal(string.Empty, seed.DisplayUrl);
            Assert.False(seed.HasPreset);
        }
    }
}
