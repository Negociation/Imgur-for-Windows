using Imgur.Contracts;
using Imgur.Models;
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


            AccountViewModel vm = new AccountViewModel(m, userContext, dialogService, navigatorService);
            return vm;
        }
    }
}
