using Imgur.Contracts;
using Imgur.Helpers;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace Imgur.Uwp.Services
{
    public class TokenService : ITokenService
    {
        private const string Resource = "ImgurOAuth";
        private readonly PasswordVault _vault = new PasswordVault();

        public bool HasToken()
        {
            try { return _vault.FindAllByResource(Resource).Count > 0; }
            catch { return false; }
        }

        public Task<StoredToken> GetTokenAsync()
        {
            try
            {
                var cred = _vault.FindAllByResource(Resource)[0];
                cred.RetrievePassword();

                if (!TokenVaultPayload.TryParse(cred.Password, out var access, out var refresh, out var expiresAt))
                    return Task.FromResult<StoredToken>(null);

                return Task.FromResult(new StoredToken
                {
                    AccessToken = access,
                    RefreshToken = refresh,
                    ExpiresAtUnix = expiresAt,
                    AccountName = cred.UserName
                });
            }
            catch { return Task.FromResult<StoredToken>(null); }
        }

        public Task SaveTokenAsync(StoredToken token)
        {
            RemoveFromVault();
            _vault.Add(new PasswordCredential(
                Resource,
                token.AccountName,
                TokenVaultPayload.Serialize(token.AccessToken, token.RefreshToken, token.ExpiresAtUnix)
            ));
            return Task.CompletedTask;
        }

        public Task RemoveTokenAsync()
        {
            RemoveFromVault();
            return Task.CompletedTask;
        }

        private void RemoveFromVault()
        {
            try
            {
                foreach (var c in _vault.FindAllByResource(Resource))
                    _vault.Remove(c);
            }
            catch { }
        }
    }
}
