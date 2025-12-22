using Imgur.Models;
using Imgur.ViewModels.Media;

namespace Imgur.Factories
{
    public interface IMediaVmFactory
    {
        MediaViewModel GetMediaViewModel(Media m);
    }
}
