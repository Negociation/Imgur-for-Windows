using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response.Account;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Mappers;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Services
{
    public class AccountService
    {

        private ILocalSettings _localSettings;
        private IAccountService _apiService;
        private AccountMapper _accountMapper;

        public AccountService(
           ILocalSettings localSettings,
           IAccountService apiService,
           AccountMapper accountMapper
           )
        {
            _localSettings = localSettings;
            _apiService = apiService;
            _accountMapper = accountMapper;
        }

        public async Task<UserAccount> GetAccountById(string id)
        {
            var retrievedAccount = await _apiService.GetAccountAsync(id);

            if (!retrievedAccount.Success)
            {
                throw new Exception("Erro busca");
            }

            return this._accountMapper.ToUserAccount(retrievedAccount.Data);

           
        }

        public async Task<Result<IReadOnlyList<UserAccount>>> SearchAccounts(string query, int page = 0)
        {
            ApiResponse<List<AccountResponse>> items = null;
            items = await _apiService.AccountSearchAsync(query, page);

            if (!items.Success)
            {
                return Result<IReadOnlyList<UserAccount>>.Failure(items.Status.ToString(), ErrorType.Server);

            }

            Debug.WriteLine(items.Data.Count);
            var tagListMapped = _accountMapper.ToUserAccountList(items.Data);
            return Result<IReadOnlyList<UserAccount>>.Success(tagListMapped);
        }
    }
}
