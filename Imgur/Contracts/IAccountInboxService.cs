using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    /// <summary>Account notifications inbox: unread badge + full dump for local paging.</summary>
    public interface IAccountInboxService
    {
        bool HasUnreadNotifications { get; }

        event EventHandler UnreadChanged;

        Task RefreshUnreadAsync();

        /// <summary>Full inbox (replies + messages), newest first. Used for local page slicing.</summary>
        Task<IReadOnlyList<AccountNotification>> GetInboxAsync(bool onlyNew = false);

        Task MarkAllReadAsync();
    }
}
