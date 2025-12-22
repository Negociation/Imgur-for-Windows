using Imgur.Models;
using Imgur.ViewModels.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Factories
{
    public interface ITagVmFactory
    {
       TagViewModel GetTagViewModel(Tag t);
    }
}
