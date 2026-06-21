using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response.Account;
using Imgur.Api.Services.Models.Response.Auth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Actions
{
    public class AuthApiService : ApiService, IAuthApiService
    {
        public AuthApiService(string clientId) : base(clientId) { }

        // ── Valida token batendo em account/me com Bearer ──────
        public async Task<ApiResponse<AccountResponse>> ValidateTokenAsync(string accessToken)
        {
            var response = await GetAsync<AccountResponse>("account/me", accessToken);

            var json = JsonConvert.SerializeObject(response, Formatting.Indented);
            Debug.WriteLine(json);

            return response;
        }

        // ── Renova o token via oauth2/token ────────────────────
        public async Task<ApiResponse<TokenData>> RefreshTokenAsync(
            string refreshToken,
            string clientId,
            string clientSecret)
        {
            try
            {
                // Endpoint fora do /3/ — HttpClient direto
                using (var http = new HttpClient())
                {
                    var body = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("refresh_token", refreshToken),
                        new KeyValuePair<string, string>("client_id",     clientId),
                        new KeyValuePair<string, string>("client_secret", clientSecret),
                        new KeyValuePair<string, string>("grant_type",    "refresh_token"),
                    });

                    var response = await http.PostAsync(
                        "https://api.imgur.com/oauth2/token", body);

                    var json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"[AuthApiService] Refresh falhou: {(int)response.StatusCode}");
                        return new ApiResponse<TokenData>
                        {
                            Success = false,
                            Status = (int)response.StatusCode
                        };
                    }

                    return new ApiResponse<TokenData>
                    {
                        Success = true,
                        Status = 200,
                        Data = new TokenData
                        {
                            AccessToken = ExtractJson(json, "access_token"),
                            RefreshToken = ExtractJson(json, "refresh_token"),
                            AccountName = ExtractJson(json, "account_username")
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthApiService] Erro no refresh: {ex.Message}");
                return new ApiResponse<TokenData> { Success = false, Status = 0 };
            }
        }

        // ── Helper ─────────────────────────────────────────────
        private static string ExtractJson(string json, string key)
        {
            var search = $"\"{key}\":\"";
            int s = json.IndexOf(search);
            if (s < 0) return null;
            s += search.Length;
            int e = json.IndexOf('"', s);
            return e < 0 ? null : json.Substring(s, e - s);
        }
    }
}