using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface ITokenService
    {
        Task<string> GetTokenAsync();
        Task SaveTokenAsync(string token);
        Task RemoveTokenAsync();
        bool HasToken();
    }
}
