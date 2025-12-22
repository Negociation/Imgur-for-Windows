using System;
using System.Collections.Generic;
using System.Text;
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

            return new TagViewModel(t, mediaVmFactory);
        }
    }
}
