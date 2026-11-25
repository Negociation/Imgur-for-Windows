using Imgur.Api.Services.Contracts;
using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Imgur.Uwp.Services
{
    public class AccountInboxService : IAccountInboxService
    {
        private readonly IAccountService _accountApi;
        private readonly IUserContext _userContext;
        private bool _hasUnread;

        public AccountInboxService(IAccountService accountApi, IUserContext userContext)
        {
            _accountApi = accountApi;
            _userContext = userContext;
        }

        public bool HasUnreadNotifications => _hasUnread;

        public event EventHandler UnreadChanged;

        public async Task RefreshUnreadAsync()
        {
            if (_userContext == null || !_userContext.IsAuthenticated)
            {
                SetUnread(false);
                return;
            }

            try
            {
                var response = await _accountApi.GetNotificationsAsync(onlyNew: true);
                if (response == null || !response.Success || response.Data == null)
                {
                    SetUnread(false);
                    return;
                }

                var flat = AccountNotificationMapper.Flatten(response.Data);
                SetUnread(flat.Count > 0);
                Debug.WriteLine($"[AccountInbox] unread count={flat.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AccountInbox] RefreshUnread: {ex.Message}");
            }
        }

        public async Task<IReadOnlyList<AccountNotification>> GetInboxAsync(bool onlyNew = false)
        {
            if (_userContext == null || !_userContext.IsAuthenticated)
                return new List<AccountNotification>();

            var response = await _accountApi.GetNotificationsAsync(onlyNew);
            if (response == null || !response.Success || response.Data == null)
                return new List<AccountNotification>();

            return AccountNotificationMapper.Flatten(response.Data);
        }

        public async Task MarkAllReadAsync()
        {
            if (_userContext == null || !_userContext.IsAuthenticated)
            {
                SetUnread(false);
                return;
            }

            try
            {
                var unread = await GetInboxAsync(onlyNew: true);
                var ids = unread.Select(n => n.Id).Where(id => id > 0).ToList();
                if (ids.Count > 0)
                {
                    var result = await _accountApi.MarkNotificationsViewedAsync(ids);
                    if (!result.Success)
                    {
                        // Fallback: mark one-by-one
                        foreach (var id in ids)
                            await _accountApi.MarkNotificationViewedAsync(id);
                    }
                }

                SetUnread(false);
                Debug.WriteLine($"[AccountInbox] MarkAllRead count={ids.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AccountInbox] MarkAllRead: {ex.Message}");
                SetUnread(false);
            }
        }

        private void SetUnread(bool value)
        {
            if (_hasUnread == value)
                return;
            _hasUnread = value;
            UnreadChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
