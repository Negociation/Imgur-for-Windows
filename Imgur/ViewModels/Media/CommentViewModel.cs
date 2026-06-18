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

        //***************************************************************
        // Constructors e Initializers
        //***************************************************************

        //-- Constructor
        public CommentViewModel(
            string galleryId,
  
            Comment comment,
            ICommentService commentService,
            IDialogService dialogService,
            IAppNotificationService appNotificationService,
            IUserContext userContext
            ) : base(dialogService, appNotificationService, userContext)
        {
            _galleryId = galleryId;
            _comment = comment;
            _commentService = commentService;
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
                                        _galleryId,
                                        reply,
                                        _commentService,
                                        _dialogService,
                                        _appNotification,
                                        _userContext
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
    }
}