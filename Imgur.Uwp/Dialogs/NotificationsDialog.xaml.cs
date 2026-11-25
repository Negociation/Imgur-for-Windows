using Imgur.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Imgur.Uwp.Dialogs
{
    public sealed partial class NotificationsDialog : ContentDialog
    {
        public NotificationsDialog()
        {
            this.InitializeComponent();
            ConfigureFullSize();
        }

        private void ConfigureFullSize()
        {
            var sysService = App.Services.GetRequiredService<ISystemInfoProvider>();
            var isMobile = sysService.IsMobile();
            var continuumMode = sysService.IsContinuum();
            if (isMobile && !continuumMode)
            {
                this.FullSizeDesired = true;
            }
        }

        private void Close_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
