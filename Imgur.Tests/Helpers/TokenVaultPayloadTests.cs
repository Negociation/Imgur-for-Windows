using Imgur.Helpers;
using Xunit;

namespace Imgur.Tests.Helpers
{
    public class TokenVaultPayloadTests
    {
        [Fact]
        public void Roundtrips_json_payload()
        {
            string payload = TokenVaultPayload.Serialize("acc", "ref", 1700000000);
            Assert.True(TokenVaultPayload.TryParse(payload, out var access, out var refresh, out var exp));
            Assert.Equal("acc", access);
            Assert.Equal("ref", refresh);
            Assert.Equal(1700000000L, exp);
        }

        [Fact]
        public void Parses_legacy_pipe_payload()
        {
            Assert.True(TokenVaultPayload.TryParse("a|b", out var access, out var refresh, out var exp));
            Assert.Equal("a", access);
            Assert.Equal("b", refresh);
            Assert.Null(exp);
        }

        [Fact]
        public void Rejects_empty()
        {
            Assert.False(TokenVaultPayload.TryParse("", out _, out _, out _));
        }
    }
}
