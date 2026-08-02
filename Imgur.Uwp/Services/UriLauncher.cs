using Imgur.Contracts;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace Imgur.Uwp.Services
{
    public class UriLauncher : IUriLauncher
    {
        public async Task<bool> LaunchAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            return await Launcher.LaunchUriAsync(uri);
        }
    }
}
