using Imgur.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Controle de Usuário está documentado em https://go.microsoft.com/fwlink/?LinkId=234236

namespace Imgur.Uwp.Controls
{
    public sealed partial class ImageViewElementControl : UserControl
    {
        public ImageViewElementControl()
        {
            this.InitializeComponent();

            //Fix for Creators Update to keep the AspectRatio
            this.SizeChanged += this.ResizeImageControl;

        }

        //
        private void ResizeImageControl(object sender, SizeChangedEventArgs e)
        {
            //Get media size
            int videoWidth = this.ImageElement.Width;
            int videoHeight = this.ImageElement.Height;
            this.Image.Width = this.ActualWidth;
            this.Image.Height = this.Image.Width * videoHeight / videoWidth;
        }


        //Dependencys
        public Element ImageElement
        {
            get
            {
                return GetValue(ImageElementProperty) as Element
                       ?? new Element();
            }
            set { SetValue(ImageElementProperty, value); }
        }

        public static readonly DependencyProperty ImageElementProperty =
            DependencyProperty.Register("ImageElement", typeof(Element), typeof(ImageViewElementControl), new PropertyMetadata(0));

    }
}

