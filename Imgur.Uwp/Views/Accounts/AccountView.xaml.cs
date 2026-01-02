using Imgur.Contracts;
using Imgur.ViewModels.Account;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imgur.Uwp.Views.Accounts
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class AccountView : Page
    {
        public AccountViewModel ViewModel => (AccountViewModel)this.DataContext;

        public AccountView()
        {
            this.InitializeComponent();
            this.LoadDesignAdapters();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.DataContext = e.Parameter as AccountViewModel;

            App.Services.GetRequiredService<IShareService>().Initialize();

            // Aplica o layout mobile/xbox correto ao navegar
            ApplyMobileLayout();
        }

        private void LoadDesignAdapters()
        {
            if (App.Services.GetRequiredService<ISystemInfoProvider>().IsMobile() ||
                App.Services.GetRequiredService<ISystemInfoProvider>().IsXbox())
            {
                Window.Current.SizeChanged += MobileWindow_SizeChanged;
            }
        }

        private async void ApplyMobileLayout()
        {
            if (App.Services.GetRequiredService<ISystemInfoProvider>().IsMobile() ||
                App.Services.GetRequiredService<ISystemInfoProvider>().IsXbox())
            {
                // Aguarda a página estar renderizada
                await Task.Delay(100);

                if (Window.Current.Bounds.Width >= 800)
                {
                    VisualStateManager.GoToState(this, "LeaveButtonMobileInlineState", false);
                }
            }
        }

        private async void MobileWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width >= 800)
            {
                await Task.Delay(100);
                VisualStateManager.GoToState(this, "LeaveButtonMobileInlineState", false);
            }
        }

    }
}
