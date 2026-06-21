using Imgur.Api.Services.Contracts;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using System;

namespace Imgur.Uwp.Services
{

    public class FolderDialogService : IFolderDialogService
    {
        public async Task<string> PickFolderAsync()
        {
            var picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            picker.FileTypeFilter.Add("*");

            var folder = await picker.PickSingleFolderAsync();

            if (folder == null)
                return null;

            return StorageApplicationPermissions.FutureAccessList.Add(folder);
        }
    }
}
