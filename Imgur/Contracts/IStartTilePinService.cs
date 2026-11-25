using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface IStartTilePinService
    {
        Task<bool> PinAsync(bool transparent = false);
    }
}
