using Imgur.Contracts;
using Imgur.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Input;
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

        // Dependency Property — ImageElement
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

        // Dependency Property — DownloadCommand (ligado externamente via binding ao MediaViewModel)
        public ICommand DownloadCommand
        {
            get => (ICommand)GetValue(DownloadCommandProperty);
            set => SetValue(DownloadCommandProperty, value);
        }

        public static readonly DependencyProperty DownloadCommandProperty =
            DependencyProperty.Register(
                "DownloadCommand",
                typeof(ICommand),
                typeof(ImageViewElementControl),
                new PropertyMetadata(null));

        //***************************************************************
        // Context Menu Handlers
        //***************************************************************

        private void CopyImageLink_Click(object sender, RoutedEventArgs e)
        {
            var link = ImageElement?.Link;
            if (string.IsNullOrEmpty(link)) return;

            App.Services.GetRequiredService<IClipboardService>().SetText(link);

            App.Services.GetRequiredService<IAppNotificationService>().AddNotification(
                new NotificationViewModel
                {
                    Message = "notification_copy_image_link_success_content"
                });
        }

        private void DownloadImage_Click(object sender, RoutedEventArgs e)
        {
            DownloadCommand?.Execute(ImageElement?.Link);
        }
    }
}