using Imgur.ViewModels.Explorer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Factories
{
    public interface IExplorerSearchVmFactory
    {
        ExplorerSearchViewModel getExplorerViewModel();

        ExplorerSearchViewModel getSearchViewModel(string query);
    }
}
