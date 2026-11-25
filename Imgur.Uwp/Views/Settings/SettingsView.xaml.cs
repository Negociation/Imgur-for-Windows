using Imgur.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Imgur.Uwp.Views.Settings
{
    public sealed partial class SettingsView : Page
    {
        private SettingsViewModel ViewModel => DataContext as SettingsViewModel;

        public SettingsView()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SettingsViewModel>();
            Loaded += SettingsView_Loaded;
        }

        private void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            SyncLanguageComboSelection();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SyncLanguageComboSelection();
        }

        private void SyncLanguageComboSelection()
        {
            if (ViewModel == null || LanguageComboBox == null)
                return;

            int index = ViewModel.SelectedLanguageIndex;
            if (index >= 0 && index < LanguageComboBox.Items.Count)
                LanguageComboBox.SelectedIndex = index;
        }
    }
}
