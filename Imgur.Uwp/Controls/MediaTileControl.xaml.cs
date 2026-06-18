using Imgur.ViewModels.Media;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace Imgur.Uwp.Controls
{
    public sealed partial class MediaTileControl : UserControl
    {
        public MediaTileControl()
        {
            this.InitializeComponent();

            this.Loaded += MediaTileControl_Loaded;
            this.Unloaded += MediaTileControl_Unloaded;
        }

        // =========================================================
        // VIEWMODEL
        // =========================================================
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(MediaViewModel),
                typeof(MediaTileControl),
                new PropertyMetadata(null));

        public MediaViewModel ViewModel
        {
            get => (MediaViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        // =========================================================
        // LIFECYCLE (recycle-safe)
        // =========================================================
        private void MediaTileControl_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreTileImageSources();
        }

        private void MediaTileControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ReleaseTileImageSources();
        }

        private void RestoreTileImageSources()
        {
            var media = ViewModel?.CurrentMedia;
            if (media == null) return;

            TrySetImageSource("mediaThumb", media.CoverImage);
            TrySetImageSource("mediaPlaceholderImage", media.CoverPlaceholder);
        }

        private void ReleaseTileImageSources()
        {
            TryClearImageSource("mediaThumb");
            TryClearImageSource("mediaPlaceholderImage");
        }

        private void TrySetImageSource(string elementName, string uri)
        {
            var image = FindChild<Image>(this, elementName);
            if (image == null) return;

            try
            {
                image.Source = string.IsNullOrWhiteSpace(uri)
                    ? null
                    : new BitmapImage(new Uri(uri));
            }
            catch
            {
                image.Source = null;
            }
        }

        private void TryClearImageSource(string elementName)
        {
            var image = FindChild<Image>(this, elementName);
            if (image != null)
                image.Source = null;
        }

        // =========================================================
        // CONTEXT MENU
        // =========================================================
        private void Media_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            CommandBarFlyout1.ShowAt(MediaElement);
        }

        // =========================================================
        // IMAGE LOADED
        // =========================================================
        private void MediaThumb_ImageOpened(object sender, RoutedEventArgs e)
        {
            if (sender is Image)
            {
                var ring = FindChild<ProgressRing>(this, "ImageLoadingRing");
                if (ring != null)
                {
                    ring.IsActive = false;
                    ring.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void MediaThumb_GifOpened(object sender, RoutedEventArgs e)
        {
            var ring = FindChild<ProgressRing>(this, "GifLoadingRing");
            if (ring != null)
            {
                ring.IsActive = false;
                ring.Visibility = Visibility.Collapsed;
            }

            var placeholder = FindChild<Grid>(this, "gifPlaceholderContainer");
            if (placeholder != null)
            {
                placeholder.Visibility = Visibility.Collapsed;
            }
        }

        // =========================================================
        // SAFE VISUAL TREE SEARCH (LOCAL ONLY)
        // =========================================================
        private T FindChild<T>(DependencyObject parent, string name)
            where T : FrameworkElement
        {
            if (parent == null) return null;

            int count = Windows.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                var child = Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is T element && element.Name == name)
                    return element;

                var result = FindChild<T>(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}