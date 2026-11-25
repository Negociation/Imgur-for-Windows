using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Imgur.Uwp.Controls
{
    public sealed partial class ProfilePhotoButton : UserControl
    {
        public ProfilePhotoButton()
        {
            this.InitializeComponent();
        }

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register(
                nameof(EditCommand),
                typeof(ICommand),
                typeof(ProfilePhotoButton),
                new PropertyMetadata(null));
    }
}
