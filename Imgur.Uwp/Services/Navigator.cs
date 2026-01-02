using Imgur.Contracts;
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Imgur.Uwp.Services
{
    public class Navigator : INavigator
    {

        public event EventHandler<string> NavigateInvoked;

        public object RootFrame { get; set; }

        /// <inheritdoc/>
        /// 

        public object Frame { get; set; }

        private List<string> NavigationStack = new List<string>();

        private bool _currentScreenMode;

        public event EventHandler<bool> FullScreenModeChanged;

        /// <inheritdoc/>
        public void GoBack(string sourcePage = null)
        {

            switch (sourcePage)
            {

                case nameof(sourcePage):
                    // supress transition to avoid implicit animation bug on home page.
                    GoBackSafely(RootFrame, new SuppressNavigationTransitionInfo());
                    break;
                default:
                    GoBackSafely(Frame);
                    break;

            }

        }

        private void GoBackSafely(object frame, NavigationTransitionInfo transition = null)
        {
            
            if (frame is Frame f && f.CanGoBack)
            {
                f.GoBack(transition);

                if (f.Content.GetType() == typeof(ExplorerView))
                {
                    f.Tag = "explorer";
                    setFullScreenMode(false);
                }
                else if (f.Content.GetType() == typeof(ExplorerSearchView))
                {
                    f.Tag = "explorerSearch";
                    setFullScreenMode(false);
                }
                else if (f.Content.GetType() == typeof(SettingsView))
                {
                    f.Tag = "settings";
                    setFullScreenMode(false);
                }
                else if (f.Content.GetType() == typeof(MediaView))
                {
                    f.Tag = "media";
                    setFullScreenMode(false);
                }
                else
                {
                    f.Tag = "unknown";
                }
                NavigateInvoked?.Invoke(this, f.Tag.ToString());
            }

        }

        public void Navigate(string name, object vm = null)
        {
            if (Frame is Frame f)
            {
                
                switch (name)
                {
                    case "explorer":
                        //Can't Navigate Twice to the same View
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

                        // Se não está na view, navega direto
                        if (currentView == null)
                        {
                            this.setFullScreenMode(false);
                            f.Tag = name;
                            f.Navigate(typeof(ExplorerSearchView), searchVm, new DrillInNavigationTransitionInfo());
                            return;
                        }

                        // Já está na view, verifica se precisa atualizar
                        var currentVm = currentView.DataContext as ExplorerSearchViewModel;

                        // Ambos querem modo Explore (sem busca)
                        if (searchVm == null && (currentVm == null || !currentVm.IsSearchMode))
                        {
                            Debug.WriteLine("Already in Explore mode");
                            return;
                        }

                        // Ambos têm busca - verifica se é a mesma
                        if (searchVm != null && currentVm != null)
                        {
                            if (currentVm.IsSearchMode == searchVm.IsSearchMode &&
                                string.Equals(currentVm.SearchQuery, searchVm.SearchQuery, StringComparison.OrdinalIgnoreCase))
                            {
                                Debug.WriteLine($"Already searching for: '{searchVm.SearchQuery}'");
                                return;
                            }
                        }

                        // Precisa navegar (mudou de modo ou query)
                        Debug.WriteLine(searchVm != null
                            ? $"Navigating to search: '{searchVm.SearchQuery}'"
                            : "Navigating to Explore mode");

                        this.setFullScreenMode(false);
                        f.Tag = name;
                        f.Navigate(typeof(ExplorerSearchView), searchVm, new DrillInNavigationTransitionInfo());
                        break;
                    case "tags":
                        //Can't Navigate Twice to the same View
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
                        this.setFullScreenMode(true);
                        f.Tag = name;
                        f.Navigate(typeof(AccountView), vm as AccountViewModel, new DrillInNavigationTransitionInfo());
                        break;
                }
                //this.NavigationStack.Add(name);
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

        public void NavigateMediaToFullScreen()
        {
            if (RootFrame is Frame f)
            {
                // f.Navigate(typeof(FullScreenMediaView), null, new DrillInNavigationTransitionInfo());
            }
        }


        public bool CanGoBack()
        {
            if (Frame is Frame f && f.CanGoBack)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public void Close()
        {
            Application.Current.Exit();
        }

        private void setFullScreenMode(bool isFullScreen)
        {
            if(this._currentScreenMode != isFullScreen)
            {
                this._currentScreenMode = isFullScreen;
                FullScreenModeChanged?.Invoke(this, isFullScreen);
            }
        }
    }
}
