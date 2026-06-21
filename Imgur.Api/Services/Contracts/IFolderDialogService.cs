using System.Threading.Tasks;

namespace Imgur.Api.Services.Contracts
{
    public interface IFolderDialogService
    {
        Task<string> PickFolderAsync();
    }
}
