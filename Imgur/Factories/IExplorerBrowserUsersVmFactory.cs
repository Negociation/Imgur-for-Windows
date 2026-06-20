using Imgur.ViewModels.Explorer;
using Imgur.ViewModels.Account;
using System.Collections.Generic;

namespace Imgur.Factories
{
    public interface IExplorerBrowserUsersVmFactory
    {
        ExplorerBrowserUsersViewModel GetViewModel(string query, List<AccountViewModel> preloadedPage0);
    }
}
