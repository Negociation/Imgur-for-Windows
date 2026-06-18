using Imgur.Api.Services.Contracts;
using Imgur.Contracts;
using Imgur.Models;
using Imgur.ViewModels.Account;
using Imgur.ViewModels.Media;
using Microsoft.Extensions.DependencyInjection; // necessário para GetRequiredService
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

        public CommentViewModel GetCommentViewModel(string galleryId, Comment comment)
        {
            var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
            var userContext = _serviceProvider.GetRequiredService<IUserContext>();
            var commentService = _serviceProvider.GetRequiredService<ICommentService>();
            var appNotificationService = _serviceProvider.GetRequiredService<IAppNotificationService>();

            return new CommentViewModel(galleryId, comment, commentService,  dialogService, appNotificationService, userContext);
        }
    }
}