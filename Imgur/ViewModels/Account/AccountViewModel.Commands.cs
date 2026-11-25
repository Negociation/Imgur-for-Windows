using Imgur.Collections;
using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Factories;
using Imgur.Helpers;
using Imgur.Models;
using Imgur.ViewModels.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Imgur.ViewModels.Account
{
    public partial class AccountViewModel
    {
        //-- Command para recarregar Posts (Submissions)
        private ICommand _reloadSubmissionsCommand;

        public ICommand ReloadSubmissionsCommand
        {
            get
            {
                if (_reloadSubmissionsCommand == null)
                {
                    _reloadSubmissionsCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            Loading = true;
                            BuildSubmissionsCollection();
                            await SubmissionsCollection.LoadNextPageAsync();
                            LoadedSuccessfully = true;
                        }
                        catch (Exception ex) { Debug.WriteLine($"[AccountVM] ReloadSubmissions: {ex.Message}"); }
                        finally { Loading = false; }
                    });
                }
                return _reloadSubmissionsCommand;
            }
        }

        //-- Command para recarregar Favoritos
        private ICommand _reloadFavoritesCommand;

        public ICommand ReloadFavoritesCommand
        {
            get
            {
                if (_reloadFavoritesCommand == null)
                {
                    _reloadFavoritesCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            Loading = true;
                            BuildFavoritesCollection();
                            await FavoritesCollection.LoadNextPageAsync();
                            LoadedSuccessfully = true;
                        }
                        catch (Exception ex) { Debug.WriteLine($"[AccountVM] ReloadFavorites: {ex.Message}"); }
                        finally { Loading = false; }
                    });
                }
                return _reloadFavoritesCommand;
            }
        }

        //-- Command para recarregar Comentários
        private ICommand _reloadCommentsCommand;

        public ICommand ReloadCommentsCommand
        {
            get
            {
                if (_reloadCommentsCommand == null)
                {
                    _reloadCommentsCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            Loading = true;
                            BuildCommentsCollection();
                            await CommentsCollection.LoadNextPageAsync();
                            LoadedSuccessfully = true;
                        }
                        catch (Exception ex) { Debug.WriteLine($"[AccountVM] ReloadComments: {ex.Message}"); }
                        finally { Loading = false; }
                    });
                }
                return _reloadCommentsCommand;
            }
        }

        //-- Command LoadMore: busca próxima página de Submissions
        private ICommand _loadMoreSubmissionsCommand;

        public ICommand LoadMoreSubmissionsCommand
        {
            get
            {
                if (_loadMoreSubmissionsCommand == null)
                {
                    _loadMoreSubmissionsCommand = new RelayCommand(async () =>
                    {
                        if (SubmissionsCollection == null || IsLoadingNewPage) return;
                        try
                        {
                            IsLoadingNewPage = true;
                            await SubmissionsCollection.LoadNextPageAsync();
                        }
                        catch (Exception ex) { Debug.WriteLine($"[AccountVM] LoadMore Submissions: {ex.Message}"); }
                        finally { IsLoadingNewPage = false; }
                    });
                }
                return _loadMoreSubmissionsCommand;
            }
        }

        //-- Command LoadMore: busca próxima página de Favorites
        private ICommand _loadMoreFavoritesCommand;

        public ICommand LoadMoreFavoritesCommand
        {
            get
            {
                if (_loadMoreFavoritesCommand == null)
                {
                    _loadMoreFavoritesCommand = new RelayCommand(async () =>
                    {
                        if (FavoritesCollection == null || IsLoadingNewPage) return;
                        try
                        {
                            IsLoadingNewPage = true;
                            await FavoritesCollection.LoadNextPageAsync();
                        }
                        catch (Exception ex) { Debug.WriteLine($"[AccountVM] LoadMore Favorites: {ex.Message}"); }
                        finally { IsLoadingNewPage = false; }
                    });
                }
                return _loadMoreFavoritesCommand;
            }
        }

        //-- Command LoadMore: busca próxima página de Comments
        private ICommand _loadMoreCommentsCommand;

        public ICommand LoadMoreCommentsCommand
        {
            get
            {
                if (_loadMoreCommentsCommand == null)
                {
                    _loadMoreCommentsCommand = new RelayCommand(async () =>
                    {
                        if (CommentsCollection == null || IsLoadingNewPage) return;
                        try
                        {
                            IsLoadingNewPage = true;
                            await CommentsCollection.LoadNextPageAsync();
                        }
                        catch (Exception ex) { Debug.WriteLine($"[AccountVM] LoadMore Comments: {ex.Message}"); }
                        finally { IsLoadingNewPage = false; }
                    });
                }
                return _loadMoreCommentsCommand;
            }
        }

        //-- Command para Follow de Autor
        //-- Command para Follow de Autor
        private ICommand _followAuthor;

        public ICommand FollowAuthor
        {
            get
            {
                if (_followAuthor == null)
                {
                    _followAuthor = new RelayCommand(async () =>
                    {
                        if (!IsAuthenticated)
                        {
                            await _dialogService.ShowLoginInterceptorDialog(LoginInterceptorEnum.FollowUser);
                            return;
                        }

                        if (UserAccountInfo.IsFollowing is null)
                        {
                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_follow_null_content"
                            });
                            return;
                        }

                        var previousValue = UserAccountInfo.IsFollowing.Value;

                        // optimistic UI
                        UserAccountInfo.IsFollowing = !previousValue;
                        IsFollowing = UserAccountInfo.IsFollowing ?? false;

                        try
                        {
                            Result<bool> result = previousValue
                                ? await _accountService.UnfollowUserAsync(UserAccountInfo.Username)
                                : await _accountService.FollowUserAsync(UserAccountInfo.Username);

                            if (!result.IsSuccess)
                            {
                                // rollback silencioso — API retornou erro mas sem exception
                                UserAccountInfo.IsFollowing = previousValue;
                                IsFollowing = previousValue;

                                _appNotification.AddNotification(new NotificationViewModel
                                {
                                    Message = "notification_follow_error_content"
                                });
                            }
                            else
                            {
                                _appNotification.AddNotification(new NotificationViewModel
                                {
                                    Message = IsFollowing
                                        ? "notification_follow_success_content"
                                        : "notification_unfollow_success_content"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[AccountVM] FollowAuthor erro: {ex.Message}");

                            // rollback seguro
                            UserAccountInfo.IsFollowing = previousValue;
                            IsFollowing = previousValue;

                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_follow_error_content"
                            });
                        }
                    });
                }
                return _followAuthor;
            }
        }


        //-- Command para abrir a midia
        private ICommand _navigateAccountViewCommand;

        public ICommand NavigateAccountViewCommand
        {
            get
            {
                if (_navigateAccountViewCommand == null)
                {
                    _navigateAccountViewCommand = new RelayCommand(async () =>
                    {
                        await Task.Delay(500);
                        _navigator.Navigate("accountView", this);
                    });
                }
                return _navigateAccountViewCommand;
            }
        }

        //-- Command New Post (empty own posts CTA)
        private ICommand _newPostCommand;

        public ICommand NewPostCommand
        {
            get
            {
                if (_newPostCommand == null)
                {
                    _newPostCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            await _dialogService.ShowUploadDialogAsync();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[AccountVM] NewPost: {ex.Message}");
                        }
                    });
                }
                return _newPostCommand;
            }
        }

        //-- Command de Back Button
        private ICommand _leaveCurrentPage;

        public ICommand LeaveCurrentPage
        {
            get
            {
                if (_leaveCurrentPage == null)
                {
                    _leaveCurrentPage = new RelayCommand(() =>
                    {
                        _navigator.GoBack();
                    });
                }
                return _leaveCurrentPage;
            }
        }

        private ICommand _openSettingsCommand;
        public ICommand OpenSettingsCommand
        {
            get
            {
                if (_openSettingsCommand == null)
                {
                    _openSettingsCommand = new RelayCommand(() =>
                    {
                        _navigator.Navigate("settings");
                    });
                }
                return _openSettingsCommand;
            }
        }

        private ICommand _openEditProfileCommand;
        public ICommand OpenEditProfileCommand
        {
            get
            {
                if (_openEditProfileCommand == null)
                {
                    _openEditProfileCommand = new RelayCommand(() =>
                    {
                        _navigator.Navigate("editprofile");
                    });
                }
                return _openEditProfileCommand;
            }
        }

        private ICommand _openTermsOfServiceCommand;
        public ICommand OpenTermsOfServiceCommand
        {
            get
            {
                if (_openTermsOfServiceCommand == null)
                {
                    _openTermsOfServiceCommand = new RelayCommand(async () =>
                    {
                        await _uriLauncher.LaunchAsync(AppUrlConstants.TermsOfServiceUrl);
                    });
                }
                return _openTermsOfServiceCommand;
            }
        }

        private ICommand _openCcpaCommand;
        public ICommand OpenCcpaCommand
        {
            get
            {
                if (_openCcpaCommand == null)
                {
                    _openCcpaCommand = new RelayCommand(async () =>
                    {
                        await _uriLauncher.LaunchAsync(AppUrlConstants.CcpaUrl);
                    });
                }
                return _openCcpaCommand;
            }
        }

        private ICommand _openHelpAndSupportCommand;
        public ICommand OpenHelpAndSupportCommand
        {
            get
            {
                if (_openHelpAndSupportCommand == null)
                {
                    _openHelpAndSupportCommand = new RelayCommand(async () =>
                    {
                        await _uriLauncher.LaunchAsync(AppUrlConstants.HelpAndSupportUrl);
                    });
                }
                return _openHelpAndSupportCommand;
            }
        }
    }
}
