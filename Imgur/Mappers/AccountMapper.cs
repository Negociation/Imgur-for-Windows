using Imgur.Api.Services.Models.Response;
using Imgur.Api.Services.Models.Response.Account;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Mappers
{
    public class AccountMapper
    {
        public UserAccount ToUserAccount(AccountResponse dto)
        {
            var account = new UserAccount
            {
                Username = dto.url,
                Avatar = dto.avatar,
                Cover = dto.cover,
                Reputation = dto.reputation,
                ReputationName = dto.reputation_name
        };

            return account;
        }
    }
}


/*
 
     if (retrievedAccount.success)
            {
                account.username = retrievedAccount.data.url;
                account.avatar = retrievedAccount.data.avatar;
                account.cover = retrievedAccount.data.cover;
                account.reputation = retrievedAccount.data.reputation;
                account.reputation_name = retrievedAccount.data.reputation_name;

                return new ContractResponse(true, account);
            }
            return new ContractResponse(false);

*/
