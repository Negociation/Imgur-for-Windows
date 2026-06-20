using Imgur.ViewModels.Explorer;
using Imgur.ViewModels.Media;
using System.Collections.Generic;

namespace Imgur.Factories
{
    public interface IExplorerBrowserGalleriesVmFactory
    {
        ExplorerBrowserGalleriesViewModel GetViewModel(string query, List<MediaViewModel> preloadedPage0);
    }
}
