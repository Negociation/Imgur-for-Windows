using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response.Account;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Contracts
{
    public interface IAccountService
    {
        Task<ApiResponse<AccountResponse>> GetAccountAsync(string id);
    }
}
