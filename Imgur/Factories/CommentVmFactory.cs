using Imgur.Api.Services.Contracts;
using Imgur.Contracts;
using Imgur.Factories;
using Imgur.Models;
using Imgur.Services;
using Imgur.ViewModels.Media;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Imgur.Factories
{
    public class CommentVmFactory : ICommentVmFactory
    {
        private readonly IServiceProvider _serviceProvider;


        public CommentVmFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public CommentViewModel GetCommentViewModel(Comment comment)
        {
            return Create(comment, galleryId: null);
        }

        public CommentViewModel GetMediaCommentViewModel(string galleryId, Comment comment)
        {
            return Create(comment, galleryId);
        }

        private CommentViewModel Create(Comment comment, string galleryId)
        {
            var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
            var userContext = _serviceProvider.GetRequiredService<IUserContext>();
            var commentService = _serviceProvider.GetRequiredService<ICommentService>();
            var appNotificationService = _serviceProvider.GetRequiredService<IAppNotificationService>();
            var clipboardService = _serviceProvider.GetRequiredService<IClipboardService>();
            var navigator = _serviceProvider.GetRequiredService<INavigator>();
            var accountVmFactory = _serviceProvider.GetRequiredService<IAccountVmFactory>();
            var accountService = _serviceProvider.GetRequiredService<AccountService>();

            return new CommentViewModel(
                comment,
                commentService,
                dialogService,
                appNotificationService,
                userContext,
                clipboardService,
                navigator,
                accountVmFactory,
                this,
                accountService,
                galleryId);
        }
    }
}
