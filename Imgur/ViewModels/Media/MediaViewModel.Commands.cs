using Imgur.Services;
using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Api.Services.Contracts;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Imgur.Models;
using Imgur.ViewModels.Account;
using Imgur.Factories;
using System.Diagnostics;
using Imgur.Enums;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Imgur.ViewModels.Base;

namespace Imgur.ViewModels.Media
{
    public partial class MediaViewModel
    {
        //-- Command para abrir a midia
        private ICommand _navigateMediaCommand;
        public ICommand NavigateMediaCommand
        {
            get
            {
                if (_navigateMediaCommand == null)
                {
                    _navigateMediaCommand = new RelayCommand(async () =>
                    {
                        await Task.Delay(100);
                        _navigator.Navigate("media", this);
                    });
                }
                return _navigateMediaCommand;
            }
        }

        //-- Command para Copiar para o Clipboard
        private ICommand _copy;
        public ICommand Copy
        {
            get
            {
                if (_copy == null)
                {
                    _copy = new RelayCommand(async () =>
                    {
                        if (await this._clipboard.GetTextAsync() != this.CurrentMedia.Link)
                        {
                            this._clipboard.SetText(this.CurrentMedia.Link);
                            NotificationViewModel notification = new NotificationViewModel();
                            notification.Message = "notification_clipboard_content";
                            this._appNotification.AddNotification(notification);
                        }
                    });
                }
                return _copy;
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

        //-- Command para Invocar Embed Dialog
        private ICommand _invokeEmbedDialog;
        public ICommand InvokeEmbedDialog
        {
            get
            {
                if (_invokeEmbedDialog == null)
                {
                    _invokeEmbedDialog = new RelayCommand(async () =>
                    {
                        await _dialogService.ShowEmbedDialogAsync(CurrentMedia);
                    });
                }
                return _invokeEmbedDialog;
            }
        }

        //-- Command para Invocar Share Dialog
        private ICommand _invokeShareDialog;
        public ICommand InvokeShareDialog
        {
            get
            {
                if (_invokeShareDialog == null)
                {
                    _invokeShareDialog = new RelayCommand(() =>
                    {
                        this._shareService.ShareMediaPost(this.CurrentMedia.Title, this.CurrentMedia.Link);
                    });
                }
                return _invokeShareDialog;
            }
        }

        //-- Command para Download de Elemento de Mídia (recebe URL como parâmetro)
        private ICommand _downloadElementCommand;
        public ICommand DownloadElementCommand
        {
            get
            {
                if (_downloadElementCommand == null)
                {
                    _downloadElementCommand = new RelayCommand<string>(async (url) =>
                    {
                        if (string.IsNullOrEmpty(url)) return;

                        try
                        {
                            var folderToken = await _folderDialogService.PickFolderAsync();
                            if (folderToken == null) return; // utilizador cancelou o picker

                            // Infere o nome do ficheiro a partir da URL
                            var uri = new Uri(url);
                            var fileName = Path.GetFileName(uri.LocalPath);
                            if (string.IsNullOrWhiteSpace(fileName))
                                fileName = $"download_{DateTime.Now:yyyyMMdd_HHmmss}";

                            await _contentStorageService.SaveAsync(uri, folderToken, fileName);

                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_download_success_content"
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[MediaVM] DownloadElementCommand erro: {ex.Message}");
                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_download_error_content"
                            });
                        }
                    });
                }
                return _downloadElementCommand;
            }
        }

        //-- Command para abrir elemento (imagem/vídeo) em fullscreen
        private ICommand _openElementFullScreenCommand;
        public ICommand OpenElementFullScreenCommand
        {
            get
            {
                if (_openElementFullScreenCommand == null)
                    _openElementFullScreenCommand = new RelayCommand<Element>(OpenElementFullScreen);
                return _openElementFullScreenCommand;
            }
        }

        //-- Command para Favoritar Midia
        private ICommand _favourite;
        public ICommand Favourite
        {
            get
            {
                if (_favourite == null)
                {
                    _favourite = new RelayCommand(async () =>
                    {
                        if (!IsAuthenticated)
                        {
                            await _dialogService.ShowLoginInterceptorDialog(LoginInterceptorEnum.Like);
                            return;
                        }

                        var previous = CurrentMedia.Favorite;
                        CurrentMedia.Favorite = !CurrentMedia.Favorite;
                        OnPropertyChanged(nameof(IsFavorited));

                        var result = await _mediaActionsService.FavoriteAsync(CurrentMedia.Id, CurrentMedia.IsAlbum);
                        if (!result.IsSuccess)
                        {
                            CurrentMedia.Favorite = previous;
                            OnPropertyChanged(nameof(IsFavorited));

                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_favourite_error_content"
                            });
                        }
                        else if (CurrentMedia.Favorite)
                        {
                            // Notifica apenas ao favoritar — ao desfavoritar não exibe nada
                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_favourite_success_content"
                            });
                        }
                    });
                }
                return _favourite;
            }
        }

        //-- Command para Vote ( Upvote ("up") | Downvote ("down") )
        private ICommand _vote;
        public ICommand Vote
        {
            get
            {
                if (_vote == null)
                {
                    _vote = new RelayCommand<string>(async (vote) =>
                    {
                        if (!IsAuthenticated)
                        {
                            await _dialogService.ShowLoginInterceptorDialog(LoginInterceptorEnum.Vote);
                            return;
                        }

                        var previous = CurrentMedia.Vote;
                        var upvoteOnly = false;
                        if (!string.IsNullOrEmpty(vote) && vote.EndsWith(":only", StringComparison.OrdinalIgnoreCase))
                        {
                            upvoteOnly = true;
                            vote = vote.Substring(0, vote.Length - ":only".Length);
                        }

                        // Double-tap / upvoteOnly: se já está up, não inverte (não remove o voto)
                        if (upvoteOnly && vote == "up" && previous == "up")
                            return;

                        CurrentMedia.Vote = CurrentMedia.Vote == vote ? null : vote;

                        OnPropertyChanged(nameof(IsUpvoted));
                        OnPropertyChanged(nameof(IsDownvoted));

                        if (vote == "up")
                        {
                            if (previous == "up") CurrentMedia.Ups--;
                            else if (previous == "down") { CurrentMedia.Downs--; CurrentMedia.Ups++; }
                            else CurrentMedia.Ups++;
                        }
                        else if (vote == "down")
                        {
                            if (previous == "down") CurrentMedia.Downs--;
                            else if (previous == "up") { CurrentMedia.Ups--; CurrentMedia.Downs++; }
                            else CurrentMedia.Downs++;
                        }

                        OnPropertyChanged(nameof(CurrentMedia));

                        var voteValue = CurrentMedia.Vote ?? "veto";
                        var result = await _mediaActionsService.VoteAsync(CurrentMedia.Id, voteValue);
                        if (!result.IsSuccess)
                        {
                            CurrentMedia.Vote = previous;
                            OnPropertyChanged(nameof(IsUpvoted));
                            OnPropertyChanged(nameof(IsDownvoted));

                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_vote_error_content"
                            });
                        }
                        else
                        {
                            CurrentMedia.Votes = CurrentMedia.Ups - CurrentMedia.Downs;
                        }
                    });
                }
                return _vote;
            }
        }

        //-- Command para Inicialização
        private ICommand _initializeCommand;
        public ICommand InitializeCommand
        {
            get
            {
                if (_initializeCommand == null)
                {
                    _initializeCommand = new RelayCommand(async () =>
                    {
                        await this.InitializeAsync();
                    });
                }
                return _initializeCommand;
            }
        }

        //-- Command para buscar e carregar comentários (parcial — primeiros 5)
        private ICommand _loadCommentsCommand;
        public ICommand LoadCommentsCommand
        {
            get
            {
                if (_loadCommentsCommand == null)
                {
                    _loadCommentsCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            LoadingComments = true;

                            var filter = _filterCommentsByNew ? "new" : "best";
                            var result = await _commentService.GetCommentsAsync(CurrentMedia.Id, filter);

                            if (result.IsSuccess)
                            {
                                _allComments = result.Data;

                                Comments.Clear();

                                foreach (var comment in _allComments.Take(5))
                                    Comments.Add(CreateCommentViewModel(comment));

                                HasComments = Comments.Count > 0;
                                HasMoreComments = _allComments.Count > 5;

                                Debug.WriteLine($"[MediaVM] {Comments.Count} comentários carregados.");
                            }
                            else
                            {
                                Debug.WriteLine($"[MediaVM] Erro ao carregar comentários: {result.ErrorType}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[MediaVM] Exceção ao carregar comentários: {ex.Message}");
                        }
                        finally
                        {
                            LoadingComments = false;
                        }
                    });
                }
                return _loadCommentsCommand;
            }
        }

        //-- Command para carregar em tela os comentários em Memória restantes
        private ICommand _loadAllCommentsCommand;
        public ICommand LoadAllCommentsCommand
        {
            get
            {
                if (_loadAllCommentsCommand == null)
                {
                    _loadAllCommentsCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            LoadingMoreComments = true;

                            await Task.Delay(300);

                            foreach (var comment in _allComments.Skip(5))
                                Comments.Add(CreateCommentViewModel(comment));

                            HasMoreComments = false;

                            Debug.WriteLine($"[MediaVM] Todos os {_allComments.Count} comentários carregados.");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[MediaVM] Exceção LoadAll: {ex.Message}");
                        }
                        finally
                        {
                            LoadingMoreComments = false;
                        }
                    });
                }
                return _loadAllCommentsCommand;
            }
        }

        //-- Command para reply em comentário
        private ICommand _replyCommentCommand;
        public ICommand ReplyCommentCommand
        {
            get
            {
                if (_replyCommentCommand == null)
                {
                    _replyCommentCommand = new RelayCommand<Comment>(async (comment) =>
                    {
                        if (!IsAuthenticated)
                        {
                            await _dialogService.ShowLoginInterceptorDialog(LoginInterceptorEnum.Comment);
                            return;
                        }
                        // TODO: abrir dialog de reply
                    });
                }
                return _replyCommentCommand;
            }
        }

        //-- Command para atualizar o filtro de comentários
        private ICommand _toggleCommentFilter;
        public ICommand ToggleCommentFilter
        {
            get
            {
                if (_toggleCommentFilter == null)
                {
                    _toggleCommentFilter = new RelayCommand(() =>
                    {
                        FilterCommentsByNew = !FilterCommentsByNew;

                        LoadCommentsCommand.Execute(null);
                    });
                }

                return _toggleCommentFilter;
            }
        }

        protected override async Task OnStartCommentAsync()
        {
            await _dialogService.ShowCommentDialog(this);
        }

        protected override async Task<bool> OnSubmitCommentAsync(string text)
        {
            var result = await _commentService.PostCommentAsync(CurrentMedia.Id, CommentText);

            await Task.Delay(1000);

            if (result.IsSuccess)
            {
                // Recarrega os comentários
                if (LoadCommentsCommand.CanExecute(null))
                    LoadCommentsCommand.Execute(null);
            }

            return result.IsSuccess;
        }

        private CommentViewModel CreateCommentViewModel(Comment comment)
        {
            var vm = _commentVmFactory.GetMediaCommentViewModel(CurrentMedia.Id, comment);
            vm.OpenAlbumPosition = OpenAlbumElementAtIndex;
            return vm;
        }

        /// <summary>
        /// Abre o elemento do álbum no índice 0-based (a partir de #N nos comentários).
        /// Se o álbum estiver parcialmente carregado e o índice ainda não existir, carrega o resto e tenta de novo.
        /// Ignora se o índice estiver fora de <see cref="Media.ImagesCount"/>.
        /// </summary>
        private async void OpenAlbumElementAtIndex(int zeroBasedIndex)
        {
            if (CurrentMedia?.Elements == null || zeroBasedIndex < 0)
                return;

            if (zeroBasedIndex >= CurrentMedia.Elements.Count)
            {
                var totalKnown = CurrentMedia.ImagesCount ?? CurrentMedia.Elements.Count;
                if (!CurrentMedia.IsAlbum || zeroBasedIndex >= totalKnown)
                    return;

                var loaded = await EnsureRemainingAlbumElementsAsync();
                if (!loaded || zeroBasedIndex >= CurrentMedia.Elements.Count)
                    return;
            }

            OpenElementFullScreen(CurrentMedia.Elements[zeroBasedIndex]);
        }

        private void OpenElementFullScreen(Element element)
        {
            if (element == null || string.IsNullOrEmpty(element.Link))
                return;

            _navigator.NavigateElementToFullScreen(new FullScreenElementArgs
            {
                Element = element,
                Post = CurrentMedia,
                AuthorUsername = UserAccount?.UserAccountInfo?.Username,
                FavoriteChanged = NotifyFavoriteChanged
            });
        }
    }
}
