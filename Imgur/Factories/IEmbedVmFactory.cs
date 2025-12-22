using Imgur.Models;
using Imgur.ViewModels.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Factories
{
    public interface IEmbedVmFactory
    {
        EmbedViewModel GetEmbedViewModel(Media m);
    }
}
