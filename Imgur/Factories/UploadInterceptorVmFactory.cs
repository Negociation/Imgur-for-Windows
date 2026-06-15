using System;
using System.Collections.Generic;
using System.Text;
using Imgur.Contracts;
using Imgur.ViewModels.FileUpload;
using Microsoft.Extensions.DependencyInjection;

namespace Imgur.Factories
{
    public class UploadInterceptorVmFactory : IUploadInterceptorVmFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public UploadInterceptorVmFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public UploadInterceptorViewModel GetUploadInterceptorViewModel()
        {
            var navigator = _serviceProvider.GetRequiredService<INavigator>();
            var localSettings = _serviceProvider.GetRequiredService<ILocalSettings>();
            var clipboardService = _serviceProvider.GetRequiredService<IClipboardService>();
            var notificationService = _serviceProvider.GetRequiredService<IAppNotificationService>();
            return new UploadInterceptorViewModel(navigator, localSettings, clipboardService, notificationService);
        }
    }
}
