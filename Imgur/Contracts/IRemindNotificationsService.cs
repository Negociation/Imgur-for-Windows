using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface IRemindNotificationsService
    {
        Task OnRemindNotificationsConfigChangedAsync();
    }
}
