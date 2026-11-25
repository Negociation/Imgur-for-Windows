using Imgur.Collections;
using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Imgur.ViewModels.Notifications
{
    public class NotificationsDialogViewModel : Observable
    {
        private readonly IAccountInboxService _inbox;
        private readonly IIncrementalCollectionFactory _collectionFactory;
        private IReadOnlyList<AccountNotification> _cache;
        private const int PageSize = 20;

        public NotificationsDialogViewModel(
            IAccountInboxService inbox,
            IIncrementalCollectionFactory collectionFactory)
        {
            _inbox = inbox;
            _collectionFactory = collectionFactory;
        }

        public IIncrementalCollection<AccountNotification> Items { get; private set; }

        public bool IsLoading { get; private set; }

        public bool HasItems => !IsLoading && _cache != null && _cache.Count > 0;

        public bool IsEmpty => !IsLoading && (_cache == null || _cache.Count == 0);

        public async Task InitializeAsync()
        {
            IsLoading = true;
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(HasItems));

            try
            {
                _cache = await _inbox.GetInboxAsync(onlyNew: false);
                Items = _collectionFactory.Create<AccountNotification>(
                    FetchLocalPageAsync,
                    pageSize: PageSize,
                    batchSize: 10,
                    autoTriggerNextPage: true);
                OnPropertyChanged(nameof(Items));

                if (_cache != null && _cache.Count > 0)
                    await Items.LoadNextPageAsync();

                // Opening the dialog marks everything as read and clears the badge.
                await _inbox.MarkAllReadAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NotificationsDialog] InitializeAsync: {ex}");
                _cache = new List<AccountNotification>();
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(IsLoading));
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(HasItems));
            }
        }

        private Task<IReadOnlyList<AccountNotification>> FetchLocalPageAsync(int page, CancellationToken ct)
        {
            var list = _cache ?? new List<AccountNotification>();
            var skip = page * PageSize;
            if (skip >= list.Count)
                return Task.FromResult((IReadOnlyList<AccountNotification>)new List<AccountNotification>());

            var take = list.Count - skip;
            if (take > PageSize)
                take = PageSize;

            var slice = new List<AccountNotification>(take);
            for (int i = 0; i < take; i++)
                slice.Add(list[skip + i]);

            return Task.FromResult((IReadOnlyList<AccountNotification>)slice);
        }
    }
}
