using Imgur.Collections;
using Imgur.Contracts;
using Imgur.Services;
using Imgur.ViewModels.Explorer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Factories
{
    public class ExplorerSearchVmFactory: IExplorerSearchVmFactory
    {

        private readonly IServiceProvider _serviceProvider;

        public ExplorerSearchVmFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public ExplorerSearchViewModel getExplorerViewModel()
        {
            var vm = this.getViewModel();

            vm.Initialize();

            return vm;
        }

        public ExplorerSearchViewModel getSearchViewModel(string query)
        {

            var vm = this.getViewModel();

            vm.InitializeSearch(query);

            return vm;
        }

        private ExplorerSearchViewModel getViewModel()
        {
            var navigator = _serviceProvider.GetRequiredService<INavigator>();
            var dispatcher = _serviceProvider.GetRequiredService<IDispatcher>();
            var tagsService = _serviceProvider.GetRequiredService<TagsService>();
            var tagVmFactory = _serviceProvider.GetRequiredService<ITagVmFactory>();
            var galleryService = _serviceProvider.GetRequiredService<GalleryService>();
            var mediaVmFactory = _serviceProvider.GetRequiredService<IMediaVmFactory>();
            var localSettings = _serviceProvider.GetRequiredService<ILocalSettings>();
            var accountService = _serviceProvider.GetRequiredService<AccountService>();
            var accountVmFactory = _serviceProvider.GetRequiredService<IAccountVmFactory>();
            var collectionFactory = _serviceProvider.GetRequiredService<IIncrementalCollectionFactory>();
            var browserTagsVmFactory = _serviceProvider.GetRequiredService<IExplorerBrowserTagsVmFactory>();
            var browserGalleriesVmFactory = _serviceProvider.GetRequiredService<IExplorerBrowserGalleriesVmFactory>();
            var browserUsersVmFactory = _serviceProvider.GetRequiredService<IExplorerBrowserUsersVmFactory>();
            var vm = new ExplorerSearchViewModel
            (
                dispatcher,
                navigator,
                tagsService,
                tagVmFactory,
                galleryService,
                mediaVmFactory,
                localSettings,
                accountService,
                accountVmFactory,
                collectionFactory,
                browserTagsVmFactory,
                browserGalleriesVmFactory,
                browserUsersVmFactory
            );

            return vm;
        }
    }
}
