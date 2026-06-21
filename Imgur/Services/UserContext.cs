using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Response.Auth;
using Imgur.Contracts;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Imgur.Services
{
    public class UserContext : IUserContext
    {
        // ── Dependências ───────────────────────────────────────
        private readonly ITokenService _tokenService;
        private readonly IAuthBroker _authBroker;
        private readonly IImgurApiCredentialsProvider _credentials;
        private readonly IAuthApiService _authApiService;

        // ── Estado ─────────────────────────────────────────────
        private User _currentUser;

        public User CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                OnAuthenticationChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsAuthenticated => _currentUser != null && _tokenService.HasToken();

        public event EventHandler OnAuthenticationChanged;

        // ── Construtor ─────────────────────────────────────────
        public UserContext(
            ITokenService tokenService,
            IAuthBroker authBroker,
            IImgurApiCredentialsProvider credentials,
            IAuthApiService authApiService)
        {
            _tokenService = tokenService;
            _authBroker = authBroker;
            _credentials = credentials;
            _authApiService = authApiService;
        }

        // ── URL de autorização ─────────────────────────────────
        private string AuthorizeUrl =>
            $"https://api.imgur.com/oauth2/authorize" +
            $"?client_id={_credentials.ClientId}" +
            $"&response_type=token" +
            $"&state=uwp";

        // ── Startup ────────────────────────────────────────────
        public async Task InitAsync()
        {
            if (!_tokenService.HasToken())
            {
                Debug.WriteLine("[UserContext] Nenhum token salvo.");
                return;
            }

            var token = await _tokenService.GetTokenAsync();
            if (token == null) return;
            //CurrentUser = new User { Username = token.AccountName, AccessToken = token.AccessToken };
            Debug.WriteLine($"[UserContext] Token encontrado para: {token.AccountName}");

            bool valid = await ValidateTokenAsync(token.AccessToken);
            if (valid)
            {
                await populateCurrentUser(token.AccessToken);
                Debug.WriteLine("[UserContext] Token válido.");
                return;
            }

            Debug.WriteLine("[UserContext] Token expirado, tentando renovar...");

            var renewed = await RefreshTokenAsync(token.RefreshToken);
            if (renewed != null)
            {

                var storedToken = new StoredToken
                {
                    AccessToken = renewed.AccessToken,
                    RefreshToken = renewed.RefreshToken,
                    AccountName = renewed.AccountName
                };
                await _tokenService.SaveTokenAsync(storedToken);
                await populateCurrentUser(storedToken.AccessToken);
                //CurrentUser = new User { Username = renewed.AccountName, AccessToken = storedToken.AccessToken };
                Debug.WriteLine("[UserContext] Token renovado com sucesso.");
            }
            else
            {
                await _tokenService.RemoveTokenAsync();
                CurrentUser = null;
                Debug.WriteLine("[UserContext] Refresh falhou. Usuário deslogado.");
            }
        }

        private async Task populateCurrentUser(string accessToken)
        {
            var profile = await _authApiService.ValidateTokenAsync(accessToken);
            if (profile.Success)
            {
                CurrentUser = new User
                {
                    Username = profile.Data.url,
                    Avatar = profile.Data.avatar,
                    Cover = profile.Data.cover,
                    AccessToken = accessToken,
                    Reputation = profile.Data.reputation,
                    ReputationName = profile.Data.reputation_name
                };
                Debug.WriteLine($"[UserContext] Perfil carregado: {CurrentUser.Username}");
            }
        }

        // ── Login ──────────────────────────────────────────────
        public async Task<bool> LoginAsync()
        {
            Debug.WriteLine("[UserContext] Iniciando login OAuth...");

            var responseData = await _authBroker.AuthenticateAsync(AuthorizeUrl);

            if (responseData == null)
            {
                Debug.WriteLine("[UserContext] Login cancelado ou falhou.");
                return false;
            }

            var token = ParseFragment(responseData);

            if (string.IsNullOrEmpty(token?.AccessToken))
            {
                Debug.WriteLine("[UserContext] Fragment inválido.");
                return false;
            }

            var storedToken = new StoredToken
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                AccountName = token.AccountName
            };
            await _tokenService.SaveTokenAsync(storedToken);
            //CurrentUser = new User { Username = token.AccountName, AccessToken = token.AccessToken };
            await populateCurrentUser(token.AccessToken);

            Debug.WriteLine($"[UserContext] Login OK: {token.AccountName}");
            return true;
        }

        // ── Logout ─────────────────────────────────────────────
        public async Task LogoutAsync()
        {
            await _tokenService.RemoveTokenAsync();
            CurrentUser = null;
            Debug.WriteLine("[UserContext] Usuário deslogado.");
        }

        // ── Validação — delega pro AuthApiService ──────────────
        private async Task<bool> ValidateTokenAsync(string accessToken)
        {
            var response = await _authApiService.ValidateTokenAsync(accessToken);
            Debug.WriteLine($"[UserContext] Validação: {response.Status}");
            return response.Success;
        }

        // ── Refresh — delega pro AuthApiService ────────────────
        private async Task<TokenData> RefreshTokenAsync(string refreshToken)
        {
            var response = await _authApiService.RefreshTokenAsync(
                refreshToken,
                _credentials.ClientId,
                _credentials.ClientSecret);

            return response.Success ? response.Data : null;
        }

        // ── ParseFragment ──────────────────────────────────────
        private static TokenData ParseFragment(string responseData)
        {
            try
            {
                var fragment = new Uri(responseData).Fragment.TrimStart('#');
                var map = new Dictionary<string, string>();

                foreach (var part in fragment.Split('&'))
                {
                    var kv = part.Split('=');
                    if (kv.Length == 2)
                        map[kv[0]] = Uri.UnescapeDataString(kv[1]);
                }

                return new TokenData
                {
                    AccessToken = map.ContainsKey("access_token") ? map["access_token"] : null,
                    RefreshToken = map.ContainsKey("refresh_token") ? map["refresh_token"] : null,
                    AccountName = map.ContainsKey("account_username") ? map["account_username"] : null,
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UserContext] Erro no ParseFragment: {ex.Message}");
                return null;
            }
        }
    }
}