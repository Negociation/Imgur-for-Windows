using Imgur.Enums;
using Imgur.Models;
using Imgur.ViewModels.Settings;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface IDialogService
    {
        Task<bool?> ShowEmbedDialogAsync(Media media);

        Task<bool?> ShowLoginInterceptorDialog(LoginInterceptorEnum loginMessage);
        Task<bool?> ShowCustomApiKeyDialog(SettingsViewModel settingsVm);
    }
}
