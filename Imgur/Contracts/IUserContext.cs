using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface IUserContext
    {
        // Estado
        bool IsAuthenticated { get; }
        User CurrentUser { get; }

        // Eventos
        event EventHandler OnAuthenticationChanged;

        // Actions
        Task<Result<bool>> LoginAsync(string username, string password);

        Task<Result<bool>> LogoutAsync(string username, string password);
    }
}
