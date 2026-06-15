using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface IUserContext
    {
        User CurrentUser { get; }
        bool IsAuthenticated { get; }

        event EventHandler OnAuthenticationChanged;

        Task InitAsync();
        Task<bool> LoginAsync();
        Task LogoutAsync();
    }
}
