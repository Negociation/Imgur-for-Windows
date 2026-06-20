using Imgur.ViewModels.Explorer;
using Imgur.ViewModels.Tags;
using System.Collections.Generic;

namespace Imgur.Factories
{
    public interface IExplorerBrowserTagsVmFactory
    {
        ExplorerBrowserTagsViewModel GetViewModel(string query, List<TagViewModel> preloadedPage0);
    }
}
