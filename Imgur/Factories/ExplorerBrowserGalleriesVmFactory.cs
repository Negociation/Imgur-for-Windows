using Imgur.Collections;
using Imgur.Contracts;
using Imgur.Services;
using Imgur.ViewModels.Explorer;
using Imgur.ViewModels.Media;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Imgur.Factories
{
    public class ExplorerBrowserGalleriesVmFactory : IExplorerBrowserGalleriesVmFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ExplorerBrowserGalleriesVmFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public ExplorerBrowserGalleriesViewModel GetViewModel(string query, List<MediaViewModel> preloadedPage0)
        {
            return new ExplorerBrowserGalleriesViewModel(
                _serviceProvider.GetRequiredService<IDispatcher>(),
                _serviceProvider.GetRequiredService<INavigator>(),
                _serviceProvider.GetRequiredService<GalleryService>(),
                _serviceProvider.GetRequiredService<IMediaVmFactory>(),
                _serviceProvider.GetRequiredService<IIncrementalCollectionFactory>(),
                query,
                preloadedPage0);
        }
    }
}
