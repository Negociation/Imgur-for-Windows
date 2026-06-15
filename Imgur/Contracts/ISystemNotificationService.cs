using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface ISystemNotificationService
    {
        void ShowUploadInProgress(string title = null);
        void ShowUploadCompleted();
        void ShowUploadFailed();
        void ShowShareTargetBlocked();
    }
}