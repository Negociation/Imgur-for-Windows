using Imgur.Contracts;
using Imgur.Factories;
using Imgur.Models;
using Imgur.Uwp.Views.Accounts;
using Imgur.Uwp.Views.Explorer;
using Imgur.Uwp.Views.Media;
using Imgur.Uwp.Views.Settings;
using Imgur.Uwp.Views.Shell;
using Imgur.Uwp.Views.Tags;
using Imgur.ViewModels.Account;
using Imgur.ViewModels.Explorer;
using Imgur.ViewModels.Media;
using Imgur.ViewModels.Tags;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Imgur.Uwp.Services
{
    public class Navigator : INavigator
    {
        private const string JournalFileName = "navigation_journal.json";

        public event EventHandler<string> NavigateInvoked;

        public object RootFrame { get; set; }

        public object Frame { get; set; }

        public bool PendingNavigationRestore { get; set; }

        private bool _currentScreenMode;

        public event EventHandler<bool> FullScreenModeChanged;

        public event EventHandler<double> ScrollOffsetChanged;

        public void ReportScrollOffset(double offset)
        {
            ScrollOffsetChanged?.Invoke(this, offset);
        }

        public void GoBack(string sourcePage = null)
        {
            // Fullscreen no RootFrame tem prioridade (botão da tela, hardware back, etc.)
            if (IsFullScreenMediaActive() ||
                sourcePage == "fullscreenMedia" ||
                sourcePage == nameof(FullScreenMediaView))
            {
                GoBackFromFullScreen();
                return;
            }

            GoBackSafely(Frame);
        }

        private bool IsFullScreenMediaActive()
        {
            return RootFrame is Frame root && root.Content is FullScreenMediaView;
        }

        private void GoBackFromFullScreen()
        {
            // Volta ao Shell cacheado sem disparar NavigateInvoked (evita StopMedia).
            if (RootFrame is Frame root && root.CanGoBack)
                root.GoBack(new SuppressNavigationTransitionInfo());
        }

        private void GoBackSafely(object frame, NavigationTransitionInfo transition = null)
        {
            if (frame is Frame f && f.CanGoBack)
            {
                f.GoBack(transition);
                UpdateTagFromContent(f);
                NavigateInvoked?.Invoke(this, f.Tag?.ToString() ?? "unknown");
            }
        }

        private void UpdateTagFromContent(Frame f)
        {
            if (f.Content == null)
            {
                f.Tag = "unknown";
                return;
            }

            var type = f.Content.GetType();
            if (type == typeof(ExplorerView))
            {
                f.Tag = "explorer";
                setFullScreenMode(false);
            }
            else if (type == typeof(ExplorerSearchView))
            {
                f.Tag = "explorerSearch";
                setFullScreenMode(false);
            }
            else if (type == typeof(ExplorerBrowserTagsView))
            {
                f.Tag = "explorerBrowserTags";
                setFullScreenMode(false);
            }
            else if (type == typeof(ExplorerBrowserUsersView))
            {
                f.Tag = "explorerBrowserUsers";
                setFullScreenMode(false);
            }
            else if (type == typeof(ExplorerBrowserGalleriesView))
            {
                f.Tag = "explorerBrowserGalleries";
                setFullScreenMode(false);
            }
            else if (type == typeof(TagsView))
            {
                f.Tag = "tags";
                setFullScreenMode(false);
            }
            else if (type == typeof(TagView))
            {
                f.Tag = "tag";
                setFullScreenMode(true);
            }
            else if (type == typeof(AccountView))
            {
                f.Tag = "accountView";
                setFullScreenMode(true);
            }
            else if (type == typeof(SettingsView))
            {
                f.Tag = "settings";
                setFullScreenMode(false);
            }
            else if (type == typeof(MediaView))
            {
                f.Tag = "media";
                setFullScreenMode(false);
            }
            else
            {
                f.Tag = "unknown";
            }
        }

        public void Navigate(string name, object vm = null)
        {
            if (Frame is Frame f)
            {
                switch (name)
                {
                    case "explorer":
                        if (f.Content.GetType() != typeof(ExplorerView))
                        {
                            this.setFullScreenMode(false);
                            f.Tag = name;
                            f.Navigate(typeof(ExplorerView), null, new DrillInNavigationTransitionInfo());
                        }
                        break;
                    case "explorerSearch":
                        var searchVm = vm as ExplorerSearchViewModel;
                        var currentView = f.Content as ExplorerSearchView;

                        if (currentView == null)
                        {
                            this.setFullScreenMode(false);
                            f.Tag = name;
                            f.Navigate(typeof(ExplorerSearchView), searchVm, new DrillInNavigationTransitionInfo());
                            return;
                        }

                        var currentVm = currentView.DataContext as ExplorerSearchViewModel;

                        if (searchVm == null && (currentVm == null || !currentVm.IsSearchMode))
                        {
                            Debug.WriteLine("Already in Explore mode");
                            return;
                        }

                        if (searchVm != null && currentVm != null)
                        {
                            if (currentVm.IsSearchMode == searchVm.IsSearchMode &&
                                string.Equals(currentVm.SearchQuery, searchVm.SearchQuery, StringComparison.OrdinalIgnoreCase))
                            {
                                Debug.WriteLine($"Already searching for: '{searchVm.SearchQuery}'");
                                return;
                            }
                        }

                        Debug.WriteLine(searchVm != null
                            ? $"Navigating to search: '{searchVm.SearchQuery}'"
                            : "Navigating to Explore mode");

                        this.setFullScreenMode(false);
                        f.Tag = name;
                        f.Navigate(typeof(ExplorerSearchView), searchVm, new DrillInNavigationTransitionInfo());
                        break;
                    case "explorerBrowserTags":
                        this.setFullScreenMode(false);
                        f.Tag = name;
                        f.Navigate(typeof(ExplorerBrowserTagsView), vm as ExplorerBrowserTagsViewModel, new DrillInNavigationTransitionInfo());
                        break;
                    case "explorerBrowserUsers":
                        this.setFullScreenMode(false);
                        f.Tag = name;
                        f.Navigate(typeof(ExplorerBrowserUsersView), vm as ExplorerBrowserUsersViewModel, new DrillInNavigationTransitionInfo());
                        break;
                    case "explorerBrowserGalleries":
                        this.setFullScreenMode(false);
                        f.Tag = name;
                        f.Navigate(typeof(ExplorerBrowserGalleriesView), vm as ExplorerBrowserGalleriesViewModel, new DrillInNavigationTransitionInfo());
                        break;
                    case "tags":
                        if (f.Content.GetType() != typeof(TagsView))
                        {
                            this.setFullScreenMode(false);
                            f.Tag = name;
                            f.Navigate(typeof(TagsView), null, new DrillInNavigationTransitionInfo());
                        }
                        break;
                    case "settings":
                        if (f.Content.GetType() != typeof(SettingsView))
                        {
                            this.setFullScreenMode(false);
                            f.Tag = name;
                            f.Navigate(typeof(SettingsView), null, new DrillInNavigationTransitionInfo());
                        }
                        break;
                    case "media":
                        this.setFullScreenMode(false);
                        f.Tag = name;
                        f.Navigate(typeof(MediaView), vm as MediaViewModel, new DrillInNavigationTransitionInfo());
                        break;
                    case "tag":
                        this.setFullScreenMode(true);
                        f.Tag = name;
                        f.Navigate(typeof(TagView), vm as TagViewModel, new DrillInNavigationTransitionInfo());
                        break;
                    case "accountView":
                        {
                            var incomingAccount = vm as AccountViewModel;
                            var currentAccountView = f.Content as AccountView;
                            var currentAccountVm = currentAccountView?.DataContext as AccountViewModel;

                            if (currentAccountView != null &&
                                incomingAccount != null &&
                                string.Equals(
                                    currentAccountVm?.UserAccountInfo?.Username,
                                    incomingAccount.UserAccountInfo?.Username,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                Debug.WriteLine($"Already on account: {incomingAccount.UserAccountInfo?.Username}");
                                return;
                            }

                            this.setFullScreenMode(true);
                            f.Tag = name;
                            f.Navigate(typeof(AccountView), incomingAccount, new DrillInNavigationTransitionInfo());
                        }
                        break;
                }
                NavigateInvoked?.Invoke(this, name);
            }
        }

        public void RootNavigate(string name)
        {
            if (RootFrame is Frame f)
            {
                switch (name)
                {
                    case "shutdown":
                        if (f.CurrentSourcePageType != typeof(ShutdownView))
                        {
                            f.Navigate(typeof(ShutdownView), null, new DrillInNavigationTransitionInfo());
                        }
                        break;
                }
            }
        }

        public void NavigateMediaToFullScreen(object payload = null)
        {
            if (RootFrame is Frame f)
            {
                // Como Ambie ToScreensaver: RootFrame + SuppressTransition
                f.Navigate(typeof(FullScreenMediaView), payload, new SuppressNavigationTransitionInfo());
            }
        }

        public bool CanGoBack()
        {
            if (IsFullScreenMediaActive())
                return RootFrame is Frame root && root.CanGoBack;

            if (Frame is Frame f && f.CanGoBack)
                return true;

            return false;
        }

        public void Close()
        {
            Application.Current.Exit();
        }

        public async Task SaveNavigationStateAsync()
        {
            try
            {
                if (!(Frame is Frame f) || f.Content == null)
                {
                    await ClearJournalFileAsync().ConfigureAwait(false);
                    return;
                }

                var entries = new List<NavJournalEntry>();
                foreach (PageStackEntry stackEntry in f.BackStack)
                {
                    var described = DescribePage(stackEntry.SourcePageType, stackEntry.Parameter);
                    if (described == null)
                        break;
                    entries.Add(described);
                }

                var current = DescribePage(f.CurrentSourcePageType, (f.Content as FrameworkElement)?.DataContext ?? f.Content);
                if (current == null)
                {
                    // Não restaura stack parcial com página final irrecuperável: corta no último entry válido.
                }
                else
                {
                    entries.Add(current);
                }

                if (entries.Count == 0)
                {
                    await ClearJournalFileAsync().ConfigureAwait(false);
                    return;
                }

                var json = JsonConvert.SerializeObject(entries);
                var file = await ApplicationData.Current.LocalFolder
                    .CreateFileAsync(JournalFileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, json);
                Debug.WriteLine($"[Navigator] Saved navigation journal ({entries.Count} entries)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Navigator] SaveNavigationStateAsync failed: {ex.Message}");
            }
        }

        public bool TryRestoreNavigationState()
        {
            if (!PendingNavigationRestore)
                return false;

            PendingNavigationRestore = false;

            try
            {
                if (!(Frame is Frame f))
                    return false;

                var entries = LoadJournalAsync().ConfigureAwait(true).GetAwaiter().GetResult();
                if (entries == null || entries.Count == 0)
                    return false;

                var suppress = new SuppressNavigationTransitionInfo();
                foreach (var entry in entries)
                {
                    if (!TryNavigateJournalEntry(f, entry, suppress))
                        break;
                }

                UpdateTagFromContent(f);
                NavigateInvoked?.Invoke(this, f.Tag?.ToString() ?? "explorer");
                _ = ClearJournalFileAsync();
                Debug.WriteLine($"[Navigator] Restored navigation journal");
                return f.Content != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Navigator] TryRestoreNavigationState failed: {ex.Message}");
                PendingNavigationRestore = false;
                _ = ClearJournalFileAsync();
                return false;
            }
        }

        /// <summary>
        /// Marks restore pending if a journal file exists (call on Terminated launch).
        /// </summary>
        public async Task PrepareNavigationRestoreAsync()
        {
            try
            {
                var entries = await LoadJournalAsync().ConfigureAwait(false);
                PendingNavigationRestore = entries != null && entries.Count > 0;
            }
            catch
            {
                PendingNavigationRestore = false;
            }
        }

        public Task ClearNavigationStateAsync()
        {
            PendingNavigationRestore = false;
            return ClearJournalFileAsync();
        }

        private async Task<List<NavJournalEntry>> LoadJournalAsync()
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(JournalFileName) as StorageFile;
                if (file == null)
                    return null;

                var json = await FileIO.ReadTextAsync(file);
                if (string.IsNullOrWhiteSpace(json))
                    return null;

                return JsonConvert.DeserializeObject<List<NavJournalEntry>>(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Navigator] LoadJournalAsync failed: {ex.Message}");
                return null;
            }
        }

        private async Task ClearJournalFileAsync()
        {
            try
            {
                var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(JournalFileName);
                if (item != null)
                    await item.DeleteAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Navigator] ClearJournalFileAsync failed: {ex.Message}");
            }
        }

        private static NavJournalEntry DescribePage(Type pageType, object parameter)
        {
            if (pageType == typeof(ExplorerView))
                return new NavJournalEntry { Key = "explorer" };

            if (pageType == typeof(SettingsView))
                return new NavJournalEntry { Key = "settings" };

            if (pageType == typeof(TagsView))
                return new NavJournalEntry { Key = "tags" };

            if (pageType == typeof(ExplorerSearchView))
            {
                var vm = parameter as ExplorerSearchViewModel
                         ?? (parameter as FrameworkElement)?.DataContext as ExplorerSearchViewModel;
                return new NavJournalEntry
                {
                    Key = "explorerSearch",
                    Payload = vm?.SearchQuery ?? string.Empty
                };
            }

            if (pageType == typeof(MediaView))
            {
                var vm = parameter as MediaViewModel
                         ?? (parameter as FrameworkElement)?.DataContext as MediaViewModel;
                var media = vm?.CurrentMedia;
                if (string.IsNullOrWhiteSpace(media?.Id))
                    return null;
                return new NavJournalEntry
                {
                    Key = "media",
                    Payload = media.Id + "|" + (media.IsAlbum ? "1" : "0")
                };
            }

            if (pageType == typeof(AccountView))
            {
                var vm = parameter as AccountViewModel
                         ?? (parameter as FrameworkElement)?.DataContext as AccountViewModel;
                var username = vm?.UserAccountInfo?.Username;
                if (string.IsNullOrWhiteSpace(username))
                    return null;
                return new NavJournalEntry { Key = "accountView", Payload = username };
            }

            if (pageType == typeof(TagView))
            {
                var vm = parameter as TagViewModel
                         ?? (parameter as FrameworkElement)?.DataContext as TagViewModel;
                var tagId = vm?.CurrentTag?.Id ?? vm?.CurrentTag?.Title;
                if (string.IsNullOrWhiteSpace(tagId))
                    return null;
                return new NavJournalEntry
                {
                    Key = "tag",
                    Payload = tagId + "|" + (vm.CurrentTag?.Title ?? tagId)
                };
            }

            // Browser lists depend on non-serializable VMs — stop journal before them.
            return null;
        }

        private bool TryNavigateJournalEntry(Frame f, NavJournalEntry entry, NavigationTransitionInfo transition)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
                return false;

            var sp = App.Services;

            switch (entry.Key)
            {
                case "explorer":
                    f.Navigate(typeof(ExplorerView), null, transition);
                    return true;

                case "settings":
                    f.Navigate(typeof(SettingsView), null, transition);
                    return true;

                case "tags":
                    f.Navigate(typeof(TagsView), null, transition);
                    return true;

                case "explorerSearch":
                    {
                        var factory = sp.GetRequiredService<IExplorerSearchVmFactory>();
                        var vm = string.IsNullOrWhiteSpace(entry.Payload)
                            ? factory.getExplorerViewModel()
                            : factory.getSearchViewModel(entry.Payload);
                        f.Navigate(typeof(ExplorerSearchView), vm, transition);
                        return true;
                    }

                case "media":
                    {
                        var parts = (entry.Payload ?? string.Empty).Split('|');
                        if (parts.Length < 1 || string.IsNullOrWhiteSpace(parts[0]))
                            return false;
                        var media = new Media
                        {
                            Id = parts[0],
                            IsAlbum = parts.Length > 1 && parts[1] == "1",
                            Elements = new ObservableCollection<Element>()
                        };
                        var vm = sp.GetRequiredService<IMediaVmFactory>().GetMediaViewModel(media);
                        f.Navigate(typeof(MediaView), vm, transition);
                        return true;
                    }

                case "accountView":
                    {
                        if (string.IsNullOrWhiteSpace(entry.Payload))
                            return false;
                        var account = new UserAccount { Username = entry.Payload };
                        var vm = sp.GetRequiredService<IAccountVmFactory>().GetAccountViewModel(account);
                        f.Navigate(typeof(AccountView), vm, transition);
                        return true;
                    }

                case "tag":
                    {
                        var parts = (entry.Payload ?? string.Empty).Split('|');
                        if (parts.Length < 1 || string.IsNullOrWhiteSpace(parts[0]))
                            return false;
                        var tag = new Tag
                        {
                            Id = parts[0],
                            Title = parts.Length > 1 ? parts[1] : parts[0],
                            Items = new ObservableCollection<Media>()
                        };
                        var vm = sp.GetRequiredService<ITagVmFactory>().GetTagViewModel(tag);
                        f.Navigate(typeof(TagView), vm, transition);
                        return true;
                    }

                default:
                    return false;
            }
        }

        private void setFullScreenMode(bool isFullScreen)
        {
            if (this._currentScreenMode != isFullScreen)
            {
                this._currentScreenMode = isFullScreen;
                FullScreenModeChanged?.Invoke(this, isFullScreen);
            }
        }

        private sealed class NavJournalEntry
        {
            public string Key { get; set; }
            public string Payload { get; set; }
        }
    }
}
