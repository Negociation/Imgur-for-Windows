using Imgur.Collections;
using Imgur.Contracts;
using Imgur.Services;
using Imgur.ViewModels.Explorer;
using Imgur.ViewModels.Account;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Imgur.Factories
{
    public class ExplorerBrowserUsersVmFactory : IExplorerBrowserUsersVmFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ExplorerBrowserUsersVmFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public ExplorerBrowserUsersViewModel GetViewModel(string query, List<AccountViewModel> preloadedPage0)
        {
            return new ExplorerBrowserUsersViewModel(
                _serviceProvider.GetRequiredService<IDispatcher>(),
                _serviceProvider.GetRequiredService<INavigator>(),
                _serviceProvider.GetRequiredService<AccountService>(),
                _serviceProvider.GetRequiredService<IAccountVmFactory>(),
                _serviceProvider.GetRequiredService<IIncrementalCollectionFactory>(),
                query,
                preloadedPage0);
        }
    }
}
