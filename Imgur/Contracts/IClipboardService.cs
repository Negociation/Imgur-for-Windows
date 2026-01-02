using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface IClipboardService
    {

        void SetText(string text);

        Task<string> GetTextAsync();

        void StartMonitoring();
        void StopMonitoring();
    }
}
