using Imgur.Collections;
using Imgur.Contracts;
using Imgur.Services;
using Imgur.ViewModels.Explorer;
using Imgur.ViewModels.Tags;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Imgur.Factories
{
    public class ExplorerBrowserTagsVmFactory : IExplorerBrowserTagsVmFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ExplorerBrowserTagsVmFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public ExplorerBrowserTagsViewModel GetViewModel(string query, List<TagViewModel> preloadedPage0)
        {
            return new ExplorerBrowserTagsViewModel(
                _serviceProvider.GetRequiredService<IDispatcher>(),
                _serviceProvider.GetRequiredService<INavigator>(),
                _serviceProvider.GetRequiredService<TagsService>(),
                _serviceProvider.GetRequiredService<ITagVmFactory>(),
                _serviceProvider.GetRequiredService<IIncrementalCollectionFactory>(),
                query,
                preloadedPage0);
        }
    }
}
