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
            if (ViewModel == null)
            {
                CommentRenderer.Render(caption, null);
                return;
            }

            CommentRenderer.Render(
                caption,
                ViewModel.Comment?.Body,
                onAlbumPositionClick: ViewModel.OpenAlbumPosition);

            ViewModel.OnRepliesRequested = () =>
            {
                // Força inicialização do elemento lazy
                this.FindName("replies");
            };
        }
    }
}
