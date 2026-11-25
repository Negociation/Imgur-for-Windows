using Imgur.Api.Services.Models.Enum;
using Imgur.Enums;
using Imgur.Models;
using Imgur.ViewModels.Account;
using Imgur.ViewModels.Media;
using Imgur.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface IDialogService
    {
        Task<bool?> ShowEmbedDialogAsync(Media media);

        Task<bool?> ShowUploadDialogAsync();
        Task<bool?> ShowUploadDialogAsync(IList<SelectedFile> preSelectedFiles = null, Action onUploadStarted = null, Action onUploadCompleted = null);

        Task<bool?> ShowLoginInterceptorDialog(LoginInterceptorEnum loginMessage);
        Task ShowUploadInterceptorDialog();

        Task<bool?> ShowCustomApiKeyDialog(SettingsViewModel settingsVm);

        Task ShowCommentDialog(MediaViewModel vm);

        Task ShowReplyDialog(CommentViewModel commentVm);

        /// <summary>Read-focused comment thread (e.g. from Account → origin post).</summary>
        Task ShowFocusedCommentDialogAsync(CommentViewModel commentVm);

        Task ShowNotificationsDialogAsync();

        /// <summary>Returns the selected preset, or null if cancelled.</summary>
        Task<ImgurAvatar?> ShowAvatarPickerDialogAsync(ImgurAvatar? current = null);
    }
}
