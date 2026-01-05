using Imgur.Models;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Imgur.Uwp.Controls
{
    public sealed partial class ImageViewElementControl : UserControl
    {
        public ImageViewElementControl()
        {
            this.InitializeComponent();
            this.SizeChanged += this.ResizeImageControl;
        }

        private void ResizeImageControl(object sender, SizeChangedEventArgs e)
        {
            if (this.ImageElement == null || this.ImageElement.Width == 0 || this.ImageElement.Height == 0)
                return;

            int originalWidth = this.ImageElement.Width;
            int originalHeight = this.ImageElement.Height;
            double containerWidth = Window.Current.Bounds.Width;

            if (containerWidth < 1000)
            {
                // 📱 MOBILE: Imagem preenche a largura disponível
                this.Image.MaxWidth = containerWidth;
                this.Image.MaxHeight = double.PositiveInfinity;
            }
            else
            {
                // 🖥️ DESKTOP: Imagem limitada ao tamanho original
                this.Image.MaxWidth = originalWidth;
                this.Image.MaxHeight = originalHeight;
            }
        }

        // Dependency Property
        public Element ImageElement
        {
            get => GetValue(ImageElementProperty) as Element ?? new Element();
            set => SetValue(ImageElementProperty, value);
        }

        public static readonly DependencyProperty ImageElementProperty =
            DependencyProperty.Register(
                "ImageElement",
                typeof(Element),
                typeof(ImageViewElementControl),
                new PropertyMetadata(null, OnImageElementChanged));

        private static void OnImageElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ImageViewElementControl;
            control?.ResizeImageControl(control, null);
        }
    }
}