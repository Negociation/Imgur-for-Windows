using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface IAccountNotificationsService
    {
        Task OnAccountNotificationsConfigChangedAsync();
    }
}
