using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response.Account;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Actions
{
    public class AccountService : ApiService, IAccountService
    {
        public AccountService(string clientId) : base(clientId)
        {
        }

        public async Task<ApiResponse<AccountResponse>> GetAccountAsync(string id)
        {
            return await GetAsync<AccountResponse>(
               $"account/{id}");
        }
    }
}
