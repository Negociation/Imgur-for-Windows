using Imgur.Services;
using Imgur.Contracts;
using Imgur.Models;
using Imgur.ViewModels.Media;
using Microsoft.Extensions.DependencyInjection; // necessário para GetRequiredService
using System;

namespace Imgur.Factories
{
    public class MediaVmFactory : IMediaVmFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MediaVmFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public MediaViewModel GetMediaViewModel(Media media)
        {
            if (media == null)
                throw new ArgumentNullException(nameof(media));

            // Resolve INavigator via DI
            var navigator = _serviceProvider.GetRequiredService<INavigator>();
            var dispatcher = _serviceProvider.GetRequiredService<IDispatcher>();
            var clipboard = _serviceProvider.GetRequiredService<IClipboardService>();
            var notification = _serviceProvider.GetRequiredService<IAppNotificationService>();
            var galleryService = _serviceProvider.GetRequiredService<GalleryService>();
            var accountService = _serviceProvider.GetRequiredService<AccountService>();
            var accountVmFactory = _serviceProvider.GetRequiredService<IAccountVmFactory>();
            var shareService = _serviceProvider.GetRequiredService<IShareService>();
            var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
            var userContext = _serviceProvider.GetRequiredService<IUserContext>();

            var vm = new MediaViewModel(
                media,
                navigator,
                dispatcher,
                clipboard,
                shareService,
                dialogService,
                notification,
                accountVmFactory,
                userContext,
                galleryService,
                accountService
                );

            vm.Initialize();
            return vm;
        }
    }
}
