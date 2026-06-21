using Imgur.Api.Services.Contracts;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Helpers;
using Imgur.Models;
using Imgur.ViewModels.Account;
using Imgur.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Imgur.ViewModels.Media
{
    public class CommentViewModel : CommentableViewModel
    {

        //***************************************************************
        // View Parameters 
        //***************************************************************

        //-- ViewStates
        private readonly string _galleryId;
        public bool IsUpvoted => Comment?.Vote == "up";
        public bool IsDownvoted => Comment?.Vote == "down";
        public bool HasReplies => Comment?.HasReplies ?? false;
        public int ReplyCount => Comment?.ReplyCount ?? 0;
        public bool RepliesVisibility => ShowReplies && HasReplies;


        private bool _repliesInitialized = false;
        
        //-- Current Comment
        private Comment _comment;
        public Comment Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(nameof(Comment)); }
        }

        //-- Respostas
        private ObservableCollection<CommentViewModel> _replies;

        public ObservableCollection<CommentViewModel> Replies
        {
            get => _replies;
            set
            {
                _replies = value;
                OnPropertyChanged(nameof(Replies));
            }
        }

        //-- Checking if is voting (async operation)
        private bool _isVoting;
        public bool IsVoting
        {
            get => _isVoting;
            set { _isVoting = value; OnPropertyChanged(nameof(IsVoting)); }
        }

        //-- If is showing replies
        private bool _showReplies;
        public bool ShowReplies
        {
            get => _showReplies;
            set { _showReplies = value; OnPropertyChanged(nameof(ShowReplies)); }
        }

        //-- Deelegado de notificação de novo comentário
        public Action OnCommentReplied;

        // Delegado de invocação das replies
        public Action OnRepliesRequested { get; set; }

        //***************************************************************
        // Services 
        //***************************************************************

        private readonly ICommentService _commentService;

        //-- Serviço de Clipboard para ações de cópia
        private readonly IClipboardService _clipboardService;

        //***************************************************************
        // Constructors e Initializers
        //***************************************************************

        //-- Constructor
        public CommentViewModel(
            Comment comment,
            ICommentService commentService,
            IDialogService dialogService,
            IAppNotificationService appNotificationService,
            IUserContext userContext,
            IClipboardService clipboardService,
            string galleryId = null
            ) : base(dialogService, appNotificationService, userContext)
        {
            _galleryId = galleryId;
            _comment = comment;
            _commentService = commentService;
            _clipboardService = clipboardService;
        }

        //-- Command para Vote ( Upvote ("up") | Downvote ("down") ) em Comentário
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

                        // Toggle — se clicar no mesmo voto remove
                        var previous = Comment.Vote;
                        Comment.Vote = Comment.Vote == vote ? null : vote;

                        // Atualiza contadores localmente
                        if (vote == "up")
                        {
                            if (previous == "up") Comment.Ups--;
                            else if (previous == "down") { Comment.Downs--; Comment.Ups++; }
                            else Comment.Ups++;
                        }
                        else if (vote == "down")
                        {
                            if (previous == "down") Comment.Downs--;
                            else if (previous == "up") { Comment.Ups--; Comment.Downs++; }
                            else Comment.Downs++;
                        }

                        OnPropertyChanged(nameof(IsUpvoted));
                        OnPropertyChanged(nameof(IsDownvoted));

                        // Sincroniza com API
                        var voteValue = Comment.Vote ?? "veto";
                        var result = await _commentService.VoteCommentAsync(Comment.Id, voteValue);

                        if (!result.Success)
                        {
                            // Reverte se falhar
                            Comment.Vote = previous;
                            OnPropertyChanged(nameof(IsUpvoted));
                            OnPropertyChanged(nameof(IsDownvoted));
                        }
                    });
                }
                return _vote;
            }
        }

        //-- Command para carregar e exibir as replies
        private ICommand _toggleRepliesCommand;
        public ICommand ToggleRepliesCommand
        {
            get
            {
                if (_toggleRepliesCommand == null)
                {
                    _toggleRepliesCommand = new RelayCommand(() =>
                    {
                        if (!HasReplies) return;

                        ShowReplies = !ShowReplies;
                        OnPropertyChanged("RepliesVisibility");
                        // Notifica a View para inicializar o lazy element
                        if (ShowReplies && !_repliesInitialized)
                        {
                            try
                            {
                                Replies = new ObservableCollection<CommentViewModel>(
                                Comment.Children.Select(reply =>
                                    new CommentViewModel(
                                        
                                        reply,
                                        _commentService,
                                        _dialogService,
                                        _appNotification,
                                        _userContext,
                                        _clipboardService,
                                        _galleryId
                                    )
                                )
                            );

                                _repliesInitialized = true;
                                OnRepliesRequested?.Invoke();
                            }
                            catch
                            {
                                Debug.WriteLine("Erro");
                            }
                        }

                        Debug.WriteLine($"[CommentVM] ShowReplies: {ShowReplies}");
                    });
                }
                return _toggleRepliesCommand;
            }
        }

        protected override async Task OnStartCommentAsync()
        {
            await this._dialogService.ShowReplyDialog(this);
        }

        protected override async Task<bool> OnSubmitCommentAsync(string text)
        {
            var result = await _commentService.ReplyCommentAsync(
            Comment.Id,
            _galleryId,
            text);

            return result.Success;
        }

        //***************************************************************
        // Commands — MoreOptions Context Menu
        //***************************************************************

        //-- Block User (TODO: implementar API)
        private ICommand _blockUserCommand;
        public ICommand BlockUserCommand
        {
            get
            {
                if (_blockUserCommand == null)
                    _blockUserCommand = new RelayCommand(() =>
                    {
                        // TODO: bloquear usuário via API
                    });
                return _blockUserCommand;
            }
        }

        //-- Report User (TODO: implementar API)
        private ICommand _reportUserCommand;
        public ICommand ReportUserCommand
        {
            get
            {
                if (_reportUserCommand == null)
                    _reportUserCommand = new RelayCommand(() =>
                    {
                        // TODO: denunciar usuário via API
                    });
                return _reportUserCommand;
            }
        }

        //-- Share (TODO: abrir diálogo de compartilhamento)
        private ICommand _shareCommand;
        public ICommand ShareCommand
        {
            get
            {
                if (_shareCommand == null)
                    _shareCommand = new RelayCommand(() =>
                    {
                        // TODO: abrir share dialog
                    });
                return _shareCommand;
            }
        }

        //-- Copy Comment Text — copia o corpo do comentário para a área de transferência
        private ICommand _copyCommentTextCommand;
        public ICommand CopyCommentTextCommand
        {
            get
            {
                if (_copyCommentTextCommand == null)
                    _copyCommentTextCommand = new RelayCommand(() =>
                    {
                        _clipboardService.SetText(Comment?.Body ?? string.Empty);
                        _appNotification.AddNotification(new NotificationViewModel
                        {
                            Message = "notification_copy_comment_success_content"
                        });
                    });
                return _copyCommentTextCommand;
            }
        }

        //-- Copy Permalink — copia o permalink do comentário (vazio por enquanto)
        private ICommand _copyPermalinkCommand;
        public ICommand CopyPermalinkCommand
        {
            get
            {
                if (_copyPermalinkCommand == null)
                    _copyPermalinkCommand = new RelayCommand(() =>
                    {
                        // TODO: construir URL real do permalink
                        _clipboardService.SetText(string.Empty);
                        _appNotification.AddNotification(new NotificationViewModel
                        {
                            Message = "notification_copy_permalink_success_content"
                        });
                    });
                return _copyPermalinkCommand;
            }
        }    }
}
