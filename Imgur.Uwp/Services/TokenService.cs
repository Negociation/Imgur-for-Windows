using Imgur.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Uwp.Services
{
    public class TokenService : ITokenService
    {
        public Task<string> GetTokenAsync()
        {
            throw new NotImplementedException();
        }

        public bool HasToken()
        {
            return false;
        }

        public Task RemoveTokenAsync()
        {
            throw new NotImplementedException();
        }

        public Task SaveTokenAsync(string token)
        {
            throw new NotImplementedException();
        }
    }
}
