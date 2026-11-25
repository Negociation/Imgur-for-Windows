using Imgur.Api.Services.Contracts;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Factories;
using Imgur.Helpers;
using Imgur.Models;
using Imgur.Services;
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
        private readonly string _galleryId;
        public bool IsUpvoted => Comment?.Vote == "up";
        public bool IsDownvoted => Comment?.Vote == "down";
        public bool HasReplies => Comment?.HasReplies ?? false;
        public int ReplyCount => Comment?.ReplyCount ?? 0;
        public bool RepliesVisibility => ShowReplies && HasReplies;

        private bool _repliesInitialized = false;

        private Comment _comment;
        public Comment Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(nameof(Comment)); }
        }

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

        private bool _isVoting;
        public bool IsVoting
        {
            get => _isVoting;
            set { _isVoting = value; OnPropertyChanged(nameof(IsVoting)); }
        }

        private bool _showReplies;
        public bool ShowReplies
        {
            get => _showReplies;
            set { _showReplies = value; OnPropertyChanged(nameof(ShowReplies)); }
        }

        public Action OnCommentReplied;
        public Action OnRepliesRequested { get; set; }

        /// <summary>
        /// Clique em #N no corpo do comentário. Índice 0-based (N-1).
        /// </summary>
        public Action<int> OpenAlbumPosition { get; set; }

        private readonly ICommentService _commentService;
        private readonly IClipboardService _clipboardService;
        private readonly INavigator _navigator;
        private readonly IAccountVmFactory _accountVmFactory;
        private readonly ICommentVmFactory _commentVmFactory;
        private readonly IMediaVmFactory _mediaVmFactory;
        private readonly AccountService _accountService;

        public CommentViewModel(
            Comment comment,
            ICommentService commentService,
            IDialogService dialogService,
            IAppNotificationService appNotificationService,
            IUserContext userContext,
            IClipboardService clipboardService,
            INavigator navigator,
            IAccountVmFactory accountVmFactory,
            ICommentVmFactory commentVmFactory,
            IMediaVmFactory mediaVmFactory,
            AccountService accountService,
            string galleryId = null
            ) : base(dialogService, appNotificationService, userContext)
        {
            _galleryId = galleryId;
            _comment = comment;
            _commentService = commentService;
            _clipboardService = clipboardService;
            _navigator = navigator;
            _accountVmFactory = accountVmFactory;
            _commentVmFactory = commentVmFactory;
            _mediaVmFactory = mediaVmFactory;
            _accountService = accountService;
        }

        /// <summary>Gallery id used for reply/vote when this VM was created for a media thread.</summary>
        public string GalleryId => _galleryId;

        /// <summary>
        /// Opens replies from already-mapped Children (Account payload may include them).
        /// </summary>
        public void ExpandRepliesForFocus()
        {
            if (!HasReplies)
                return;

            ShowReplies = true;
            OnPropertyChanged(nameof(RepliesVisibility));
            EnsureRepliesInitialized();
        }

        private void EnsureRepliesInitialized()
        {
            if (_repliesInitialized || !HasReplies)
                return;

            try
            {
                Replies = new ObservableCollection<CommentViewModel>(
                    Comment.Children.Select(reply =>
                    {
                        var replyVm = _commentVmFactory.GetMediaCommentViewModel(_galleryId, reply);
                        replyVm.OpenAlbumPosition = OpenAlbumPosition;
                        return replyVm;
                    })
                );

                _repliesInitialized = true;
                OnRepliesRequested?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CommentVM] EnsureRepliesInitialized: {ex.Message}");
            }
        }

        private ICommand _openAuthorProfileCommand;
        public ICommand OpenAuthorProfileCommand
        {
            get
            {
                if (_openAuthorProfileCommand == null)
                {
                    _openAuthorProfileCommand = new RelayCommand(async () =>
                    {
                        var username = Comment?.Author;
                        if (string.IsNullOrWhiteSpace(username) ||
                            string.Equals(username, "Hidden", StringComparison.OrdinalIgnoreCase))
                            return;

                        UserAccount account;
                        var result = await _accountService.GetAccountById(username);
                        if (result.IsSuccess && result.Data != null)
                        {
                            account = result.Data;
                            if (string.IsNullOrWhiteSpace(account.Avatar))
                                account.Avatar = Comment.AuthorAvatar;
                        }
                        else
                        {
                            account = new UserAccount
                            {
                                Username = username,
                                Avatar = Comment.AuthorAvatar
                            };
                        }

                        var vm = _accountVmFactory.GetAccountViewModel(account);
                        _navigator.Navigate("accountView", vm);
                    });
                }
                return _openAuthorProfileCommand;
            }
        }

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

                        var previous = Comment.Vote;
                        Comment.Vote = Comment.Vote == vote ? null : vote;

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

                        var voteValue = Comment.Vote ?? "veto";
                        var result = await _commentService.VoteCommentAsync(Comment.Id, voteValue);

                        if (!result.Success)
                        {
                            Comment.Vote = previous;
                            OnPropertyChanged(nameof(IsUpvoted));
                            OnPropertyChanged(nameof(IsDownvoted));
                        }
                    });
                }
                return _vote;
            }
        }

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
                        OnPropertyChanged(nameof(RepliesVisibility));
                        if (ShowReplies)
                            EnsureRepliesInitialized();

                        Debug.WriteLine($"[CommentVM] ShowReplies: {ShowReplies}");
                    });
                }
                return _toggleRepliesCommand;
            }
        }

        private ICommand _openOriginPostCommand;
        public ICommand OpenOriginPostCommand
        {
            get
            {
                if (_openOriginPostCommand == null)
                {
                    _openOriginPostCommand = new RelayCommand(() =>
                    {
                        var imageId = Comment?.ImageId;
                        if (string.IsNullOrWhiteSpace(imageId) || _mediaVmFactory == null)
                            return;

                        var childrenCount = Comment.Children?.Count ?? 0;
                        Debug.WriteLine(
                            $"[CommentVM] OpenOriginPost id={Comment.Id} imageId={imageId} onAlbum={Comment.OnAlbum} children={childrenCount}");

                        var media = CommentOriginPostHelper.CreateMedia(Comment);
                        if (media == null)
                            return;

                        var mediaVm = _mediaVmFactory.GetMediaViewModel(media);
                        var focused = _commentVmFactory.GetMediaCommentViewModel(imageId, Comment);
                        focused.ExpandRepliesForFocus();
                        mediaVm.FocusedComment = focused;
                        _navigator.Navigate("media", mediaVm);
                    });
                }
                return _openOriginPostCommand;
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

        private ICommand _blockUserCommand;
        public ICommand BlockUserCommand
        {
            get
            {
                if (_blockUserCommand == null)
                    _blockUserCommand = new RelayCommand(() => { });
                return _blockUserCommand;
            }
        }

        private ICommand _reportUserCommand;
        public ICommand ReportUserCommand
        {
            get
            {
                if (_reportUserCommand == null)
                    _reportUserCommand = new RelayCommand(() => { });
                return _reportUserCommand;
            }
        }

        private ICommand _shareCommand;
        public ICommand ShareCommand
        {
            get
            {
                if (_shareCommand == null)
                    _shareCommand = new RelayCommand(() => { });
                return _shareCommand;
            }
        }

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

        private ICommand _copyPermalinkCommand;
        public ICommand CopyPermalinkCommand
        {
            get
            {
                if (_copyPermalinkCommand == null)
                    _copyPermalinkCommand = new RelayCommand(() =>
                    {
                        _clipboardService.SetText(string.Empty);
                        _appNotification.AddNotification(new NotificationViewModel
                        {
                            Message = "notification_copy_permalink_success_content"
                        });
                    });
                return _copyPermalinkCommand;
            }
        }
    }
}
