using Imgur.Api.Services.Contracts;
using Imgur.Contracts;
using Imgur.Mappers;
using Imgur.Models;
using System;
using System.Collections.Generic;
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
    }
}
