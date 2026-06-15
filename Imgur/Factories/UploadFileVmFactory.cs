using System;
using System.Collections.Generic;
using System.Text;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Services;
using Imgur.ViewModels.Account;
using Imgur.ViewModels.FileUpload;
using Microsoft.Extensions.DependencyInjection;

namespace Imgur.Factories
{
    public class UploadFileVmFactory : IUploadFileVmFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public UploadFileVmFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }


        public UploadFileViewModel GetUploadFileViewModel()
        {
            var filePicker = _serviceProvider.GetRequiredService<IFilePicker>();
            var sysNotification = _serviceProvider.GetRequiredService<ISystemNotificationService>();
            var iuService = _serviceProvider.GetRequiredService<ImageUploadService>();
            var albumService = _serviceProvider.GetRequiredService<AlbumService>();
            var imageService = _serviceProvider.GetRequiredService<ImageService>();
            var mediaVmFactory = _serviceProvider.GetRequiredService<IMediaVmFactory>();
            var anService = _serviceProvider.GetRequiredService<IAppNotificationService>();

            return new UploadFileViewModel(filePicker, sysNotification, anService, mediaVmFactory, iuService, albumService, imageService);
        }
    }
}
