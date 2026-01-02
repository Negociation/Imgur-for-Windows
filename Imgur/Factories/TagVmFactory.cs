using System;
using System.Collections.Generic;
using System.Text;
using Imgur.Contracts;
using Imgur.Models;
using Imgur.ViewModels.Tags;
using Microsoft.Extensions.DependencyInjection;

namespace Imgur.Factories
{
    public class TagVmFactory : ITagVmFactory
    {

        private readonly IServiceProvider _serviceProvider;

        public TagVmFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public TagViewModel GetTagViewModel(Tag t)
        {
            var mediaVmFactory =  _serviceProvider.GetRequiredService<IMediaVmFactory>();
            var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
            var userContext = _serviceProvider.GetRequiredService<IUserContext>();
            var navigator = _serviceProvider.GetRequiredService<INavigator>();
            var localSettingsService = _serviceProvider.GetRequiredService<ILocalSettings>();

            return new TagViewModel(t, mediaVmFactory, dialogService, userContext, navigator, localSettingsService);
        }
    }
}
