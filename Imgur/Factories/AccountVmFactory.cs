using Imgur.Collections;
using Imgur.Contracts;
using Imgur.Models;
using Imgur.Services;
using Imgur.ViewModels.Account;
using Microsoft.Extensions.DependencyInjection; // necessário para GetRequiredService
using System;

namespace Imgur.Factories
{
    public class AccountVmFactory: IAccountVmFactory
    {

        private readonly IServiceProvider _serviceProvider;

        public AccountVmFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public AccountViewModel GetAccountViewModel(UserAccount m)
        {
            var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
            var userContext = _serviceProvider.GetRequiredService<IUserContext>();
            var navigatorService = _serviceProvider.GetRequiredService<INavigator>();
            var collectionFactory = _serviceProvider.GetRequiredService<IIncrementalCollectionFactory>();
            var accountService = _serviceProvider.GetRequiredService<AccountService>();
            var mediaVmFactory = _serviceProvider.GetRequiredService<IMediaVmFactory>();
            var commentVmFactory = _serviceProvider.GetRequiredService<ICommentVmFactory>();
            var settingsService = _serviceProvider.GetRequiredService<ILocalSettings>();
            var appNotification = _serviceProvider.GetRequiredService<IAppNotificationService>();

            AccountViewModel vm = new AccountViewModel(m, userContext, dialogService, navigatorService, settingsService, collectionFactory, accountService, mediaVmFactory, commentVmFactory, appNotification);
            return vm;
        }
    }
}
