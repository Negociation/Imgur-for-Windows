using Imgur.Api.Services.Models.Response;
using Imgur.Api.Services.Models.Response.Account;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public List<UserAccount> ToUserAccountList(List<AccountResponse> dtoList)
        {
            try
            {
                if (dtoList == null)
                {
                    Debug.WriteLine("ToAccountList: dtoList veio NULL");
                    return new List<UserAccount>();
                }

                // Mapear usando LINQ
                return dtoList.Select(item => this.ToUserAccount(item)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao converter TagResponse para Tag:");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

                // opcional: retornar lista vazia para não quebrar a UI
                return new List<UserAccount>();
            }
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
