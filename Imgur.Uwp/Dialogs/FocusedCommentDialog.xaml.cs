using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Imgur.Uwp.Dialogs
{
    public sealed partial class FocusedCommentDialog : ContentDialog
    {
        public FocusedCommentDialog()
        {
            this.InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
