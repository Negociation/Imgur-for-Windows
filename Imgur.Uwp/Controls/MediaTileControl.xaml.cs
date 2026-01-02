using Imgur.ViewModels.Media;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

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


        private FrameworkElement imageTemplate;
        private FrameworkElement gifTemplate;

        private void ImageBorder_Loaded(object sender, RoutedEventArgs e)
        {
            var border = sender as Border;
            //Debug.WriteLine($"ImageBorder_Loaded: border = {border != null}");

            // Sobe até o Grid "mediaContainer"
            var mediaContainer = FindVisualParent<Grid>(border);

            // Sobe mais um nível até o Grid ROOT do template
            imageTemplate = FindVisualParent<Grid>(mediaContainer);

            //Debug.WriteLine($"imageTemplate encontrado: {imageTemplate != null}");
        }

        private void GifBorder_Loaded(object sender, RoutedEventArgs e)
        {
            var border = sender as Border;

            // 1º nível: gifContainer
            var gifContainer = FindVisualParent<Grid>(border);

            // 2º nível: mediaContainer
            var mediaContainer = FindVisualParent<Grid>(gifContainer);

            // 3º nível: Grid ROOT (que tem o GifLoadingRing!)
            gifTemplate = FindVisualParent<Grid>(mediaContainer);

            //Debug.WriteLine($"gifTemplate encontrado: {gifTemplate != null}");
        }

        private void MediaThumb_ImageOpened(object sender, RoutedEventArgs e)
        {
            /*
            if (sender is ImageBrush brush &&
                brush.ImageSource is BitmapSource bmp)
            {
                Debug.WriteLine($"[BRUSH] Pixels: {bmp.PixelWidth} x {bmp.PixelHeight}");

                long bytes = (long)bmp.PixelWidth * bmp.PixelHeight * 4;
                Debug.WriteLine($"[BRUSH] Memória ~ {bytes / 1024d / 1024d:F2} MB");

                // Só BitmapImage tem UriSource
                if (bmp is BitmapImage bi)
                {
                    Debug.WriteLine($"[BRUSH] Source: {bi.UriSource}");
                    Debug.WriteLine($"[BRUSH] DecodePixelWidth: {bi.DecodePixelWidth}");
                    Debug.WriteLine($"[BRUSH] CreateOptions: {bi.CreateOptions}");
                }
                else
                {
                    Debug.WriteLine($"[BRUSH] ImageSource Type: {bmp.GetType().Name}");
                }
            }
            */
            if (imageTemplate == null)
            {
                //Debug.WriteLine("imageTemplate é NULL - saindo");
                return;
            }

            var progressRing = FindChildByName<ProgressRing>(imageTemplate, "ImageLoadingRing");
            //Debug.WriteLine($"ProgressRing encontrado: {progressRing != null}");

            if (progressRing != null)
            {
                //Debug.WriteLine("Parando ImageLoadingRing!");
                progressRing.IsActive = false;
                progressRing.Visibility = Visibility.Collapsed;
            }
        }

        private void MediaThumb_GifOpened(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine($"MediaThumb_GifOpened disparou! gifTemplate = {gifTemplate != null}");

            if (gifTemplate == null)
            {
                //Debug.WriteLine("gifTemplate é NULL - saindo");
                return;
            }

            var progressRing = FindChildByName<ProgressRing>(gifTemplate, "GifLoadingRing");
            //Debug.WriteLine($"GifProgressRing encontrado: {progressRing != null}");

            if (progressRing != null)
            {
                //Debug.WriteLine("Parando GifLoadingRing!");
                progressRing.IsActive = false;
                progressRing.Visibility = Visibility.Collapsed;
            }

            var placeholderContainer = FindChildByName<Grid>(gifTemplate, "gifPlaceholderContainer");
            if (placeholderContainer != null)
            {
                //Debug.WriteLine("Removendo gifPlaceholderContainer da memória");

                var parent = VisualTreeHelper.GetParent(placeholderContainer) as Grid;
                if (parent != null)
                {
                    parent.Children.Remove(placeholderContainer);
                }
            }

        }
        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T result)
                    return result;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private T FindChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T element && element.Name == name)
                    return element;

                var result = FindChildByName<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
