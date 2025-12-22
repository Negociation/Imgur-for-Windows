using Imgur.Contracts;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Services
{
    public class UserContext : IUserContext
    {

        private User _currentUser;

        public User CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                OnAuthenticationChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsAuthenticated => _currentUser != null && _tokenService.HasToken();

        public event EventHandler OnAuthenticationChanged;

        private ITokenService _tokenService;


        public UserContext(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public Task<Result<bool>> LoginAsync(string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<Result<bool>> LogoutAsync(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
