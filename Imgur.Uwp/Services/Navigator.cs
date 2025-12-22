using Imgur.Contracts;
using Imgur.Uwp.Views.Explorer;
using Imgur.Uwp.Views.Media;
using Imgur.Uwp.Views.Settings;
using Imgur.Uwp.Views.Shell;
using Imgur.Uwp.Views.Tags;
using Imgur.ViewModels.Explorer;
using Imgur.ViewModels.Media;
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
                }
                else if (f.Content.GetType() == typeof(ExplorerSearchView))
                {
                    f.Tag = "explorerSearch";
                }
                else if (f.Content.GetType() == typeof(SettingsView))
                {
                    f.Tag = "settings";
                }
                /*
                else if (f.Content.GetType() == typeof(MediaView))
                {
                    f.Tag = "media";
                }
                */
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
                            f.Tag = name;
                            f.Navigate(typeof(ExplorerView), null, new DrillInNavigationTransitionInfo());
                        }
                        break;
                    case "explorerSearch":
                        var searchVm = vm as ExplorerSearchViewModel;

                        //Can't Navigate Twice to the same View
                        if (f.Content.GetType() != typeof(ExplorerSearchView))
                        {
                            f.Tag = name;
                            f.Navigate(typeof(ExplorerSearchView), searchVm, new DrillInNavigationTransitionInfo());
                        }
                        else
                        {
                            var currentView = f.Content as ExplorerSearchView;
                            if (currentView.DataContext is ExplorerSearchViewModel currentVm && searchVm != null)
                            {
                                // Se são exatamente iguais (mesmo modo e mesma query), não navega
                                if (currentVm.IsSearchMode == searchVm.IsSearchMode &&
                                    string.Equals(currentVm.SearchQuery, searchVm.SearchQuery))
                                {
                                    return;
                                }
                                else
                                {
                                    // Navega porque mudou o modo ou a query
                                    f.Tag = name;
                                    f.Navigate(typeof(ExplorerSearchView), searchVm, new DrillInNavigationTransitionInfo());
                                }
                            }
                            else
                            {
                                f.Tag = name;
                                f.Navigate(typeof(ExplorerSearchView), null, new DrillInNavigationTransitionInfo());
                            }
                        }
                        break;
                    case "tags":
                        //Can't Navigate Twice to the same View
                        if (f.Content.GetType() != typeof(TagsView))
                        {
                            f.Tag = name;
                            f.Navigate(typeof(TagsView), null, new DrillInNavigationTransitionInfo());
                        }
                        break;
                    case "settings":
                        if (f.Content.GetType() != typeof(SettingsView))
                        {
                            f.Tag = name;
                            f.Navigate(typeof(SettingsView), null, new DrillInNavigationTransitionInfo());
                        }
                        break;
                    case "media":
                        f.Tag = name;
                        f.Navigate(typeof(MediaView), vm as MediaViewModel, new DrillInNavigationTransitionInfo());
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
    }
}
