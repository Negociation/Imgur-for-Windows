using Imgur.Uwp.Helpers;
using Imgur.ViewModels.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Imgur.Uwp.Controls
{
    public sealed partial class CommentControl : UserControl
    {
        private CommentViewModel ViewModel => DataContext as CommentViewModel;

        public CommentControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            CommentRenderer.Render(caption, ViewModel?.Comment?.Body);

            if (ViewModel == null)
                return;

            ViewModel.OnRepliesRequested = () =>
            {
                // Força inicialização do elemento lazy
                this.FindName("replies");
            };
        }
    }
}
