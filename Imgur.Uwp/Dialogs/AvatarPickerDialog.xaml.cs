using Imgur.Contracts;
using Imgur.ViewModels.Account;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Imgur.Uwp.Dialogs
{
    public sealed partial class AvatarPickerDialog : ContentDialog
    {
        public AvatarPickerViewModel ViewModel => DataContext as AvatarPickerViewModel;

        public bool Confirmed { get; private set; }

        public AvatarPickerDialog()
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
            Confirmed = false;
            this.Hide();
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null || !ViewModel.HasSelection)
                return;

            Confirmed = true;
            this.Hide();
        }
    }
}
