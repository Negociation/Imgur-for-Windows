using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response.Account;
using Imgur.Api.Services.Models.Response.Auth;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Contracts
{
    public interface IAuthApiService
    {
        Task<ApiResponse<AccountResponse>> ValidateTokenAsync(string accessToken);
        Task<ApiResponse<TokenData>> RefreshTokenAsync(string refreshToken, string clientId, string clientSecret);
    }
}