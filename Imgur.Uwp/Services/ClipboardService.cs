using Imgur.Contracts;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace Imgur.Uwp.Services
{
    public class ClipboardService : IClipboardService
    {

        private DataPackage dataPackage = new DataPackage();

        public void SetText(string text)
        {
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }

        public async Task<string> GetTextAsync()
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                return await dataPackageView.GetTextAsync();
            }
            return string.Empty;
        }

    }
}
