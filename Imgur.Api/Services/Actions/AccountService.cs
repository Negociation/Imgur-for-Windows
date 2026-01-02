using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response.Account;
using Imgur.Api.Services.Models.Response.Suggest;
using Newtonsoft.Json;
using System.Collections.Generic;
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

        public async Task<ApiResponse<List<AccountResponse>>> AccountSearchAsync(string query, int page = 0)
        {
            //Buscar Sugestões (Max 10 Itens)...
            var urlSuggest = $"suggest?inflate=users&q={query}&types=users";
            var suggestTags = await GetAsync<SuggestResponse>(urlSuggest);

            if (!suggestTags.Success)
            {
                var errorResponse = new ApiResponse<List<AccountResponse>>();
                errorResponse.Status = suggestTags.Status;
                errorResponse.Success = suggestTags.Success;
                return errorResponse;
            }

            var successResponse = new ApiResponse<List<AccountResponse>>();
            successResponse.Data = new List<AccountResponse>();
            successResponse.Success = true;
            successResponse.Status = 200;
            //Debug.WriteLine(JsonConvert.SerializeObject(suggestTags, Formatting.Indented));

            foreach (var user in suggestTags.Data.users)
            {
                var response = await GetAccountAsync(user.url);

                if (!response.Success)
                {
                    //skip
                    continue;
                }

                successResponse.Data.Add(response.Data);
            }

            return successResponse;
        }
    }
}
