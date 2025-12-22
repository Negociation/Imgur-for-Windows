using Imgur.Contracts;
using Imgur.Models;
using Imgur.ViewModels.Media;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Factories
{
    public class EmbedVmFactory : IEmbedVmFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public EmbedVmFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public EmbedViewModel GetEmbedViewModel(Media media)
        {

            if (media == null)
                throw new ArgumentNullException(nameof(media));

            var clipboardService = _serviceProvider.GetRequiredService<IClipboardService>();
            var notificationService = _serviceProvider.GetRequiredService<IAppNotificationService>();

            return new EmbedViewModel(media, clipboardService, notificationService);
        }
    }
}
