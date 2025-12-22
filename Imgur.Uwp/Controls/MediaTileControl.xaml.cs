using Imgur.ViewModels.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// O modelo de item de Controle de Usuário está documentado em https://go.microsoft.com/fwlink/?LinkId=234236

namespace Imgur.Uwp.Controls
{
    public sealed partial class MediaTileControl : UserControl
    {
        public MediaTileControl()
        {
            this.InitializeComponent();

        }

        private void Media_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            CommandBarFlyout1.ShowAt(MediaElement);
        }


        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(MediaViewModel),
            typeof(MediaTileControl),
            new PropertyMetadata(null));

        public MediaViewModel ViewModel
        {
            get => (MediaViewModel)GetValue(ViewModelProperty);
            set { SetValue(ViewModelProperty, value); }
        }


    }
}
