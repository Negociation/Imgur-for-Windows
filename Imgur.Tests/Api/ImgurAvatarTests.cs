using Imgur.Api.Services.Actions;
using Imgur.Api.Services.Models.Enum;
using Imgur.Api.Services.Models.Request;
using System.Threading.Tasks;
using Xunit;

namespace Imgur.Tests.Api
{
    public class ImgurAvatarTests
    {
        [Fact]
        public void Taco_roundtrips_api_value()
        {
            var api = ImgurAvatar.Taco.GetApiValue();
            Assert.Equal("flavor/taco", api);
            Assert.True(ImgurAvatarExtensions.TryParseApiValue(api, out var parsed));
            Assert.Equal(ImgurAvatar.Taco, parsed);
            Assert.Equal("ms-appx:///Assets/Avatars/taco.png", parsed.GetLocalImagePath());
        }

        [Fact]
        public void LetterA_uses_default_prefix()
        {
            Assert.Equal("default/A", ImgurAvatar.LetterA.GetApiValue());
            Assert.True(ImgurAvatarExtensions.IsValidApiValue("default/A"));
        }

        [Fact]
        public void Parses_bare_slug_from_avatar_name()
        {
            Assert.True(ImgurAvatarExtensions.TryParseApiValue("taco", out var parsed));
            Assert.Equal(ImgurAvatar.Taco, parsed);
        }

        [Fact]
        public void Rejects_unknown_api_value()
        {
            Assert.False(ImgurAvatarExtensions.IsValidApiValue("flavor/not-a-real-avatar"));
            Assert.False(ImgurAvatarExtensions.TryParseApiValue("", out _));
        }

        [Fact]
        public async Task UpdateAccountSettings_rejects_unknown_avatar_without_http()
        {
            var svc = new AccountService("test-client");
            var result = await svc.UpdateAccountSettingsAsync(new UpdateAccountSettingsRequest
            {
                avatar = "flavor/definitely-invalid"
            });
            Assert.False(result.Success);
            Assert.Equal(400, result.Status);
        }
    }
}
