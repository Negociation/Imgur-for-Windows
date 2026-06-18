using Imgur.ViewModels.Base;
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
    public sealed partial class CommentDialogControl : UserControl
    {
        public CommentableViewModel ViewModel => (CommentableViewModel)this.DataContext;

        public CommentDialogControl()
        {
            this.InitializeComponent();
            CommentBox.TextChanged += OnCommentTextChanged;
        }

        private void OnCommentTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.CommentText = CommentBox.Text;
        }



        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(CommentDialogControl), new PropertyMetadata(null));


    }
}
