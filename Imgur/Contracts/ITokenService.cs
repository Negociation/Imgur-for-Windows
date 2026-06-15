using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface ITokenService
    {
        Task<StoredToken> GetTokenAsync();
        Task SaveTokenAsync(StoredToken token);
        Task RemoveTokenAsync();
        bool HasToken();
    }
}
