using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface IUriLauncher
    {
        Task<bool> LaunchAsync(string url);
    }
}
