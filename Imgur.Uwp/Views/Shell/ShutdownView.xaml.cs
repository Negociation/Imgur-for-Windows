using Imgur.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imgur.Uwp.Views.Shell
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>

    public sealed partial class ShutdownView : Page
    {
        public ShutdownViewModel ViewModel => (ShutdownViewModel)this.DataContext;

        public ShutdownView()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ShutdownViewModel>();

            ViewModel.PropertyChanged += OnViewModelPropertyChanged;

        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == nameof(ViewModel.StartAnimation) && ViewModel.StartAnimation)
            {
                this.colorStoryboard.Begin();
            }
        }

        private void ColorAnimation_Completed(object sender, object e)
        {
            ViewModel.NavigateToCommand.Execute(null);
        }
    }
}