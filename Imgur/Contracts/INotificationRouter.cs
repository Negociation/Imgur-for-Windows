using Imgur.Models;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    /// <summary>
    /// Routes account notifications: InApp when the UI is foreground, toast when background.
    /// </summary>
    public interface INotificationRouter
    {
        void SetForeground(bool isForeground);

        bool IsForeground { get; }

        Task StartAsync();

        void Stop();

        /// <summary>Deliver a single newly detected notification (from poll or background handoff).</summary>
        void Deliver(AccountNotification notification);
    }
}
