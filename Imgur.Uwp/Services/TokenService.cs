using Imgur.Contracts;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                var parts = cred.Password.Split('|');

                return Task.FromResult(new StoredToken
                {
                    AccessToken = parts[0],
                    RefreshToken = parts[1],
                    AccountName = cred.UserName
                });
            }
            catch { return Task.FromResult<StoredToken>(null); }
        }

        public Task SaveTokenAsync(StoredToken token)
        {
            RemoveFromVault(); // limpa anterior
            _vault.Add(new PasswordCredential(
                Resource,
                token.AccountName,
                $"{token.AccessToken}|{token.RefreshToken}"
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
