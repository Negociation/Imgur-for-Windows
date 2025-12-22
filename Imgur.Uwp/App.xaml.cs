using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Phone.UI.Input;
using Windows.Foundation;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Popups;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Extensions.DependencyInjection;
using Imgur.Contracts;
using Imgur.Uwp.Services;
using Imgur.Uwp.Views.Shell;
using Imgur.ViewModels.Shell;
using Imgur.ViewModels.Settings;
using Imgur.ViewModels;
using Imgur.Api.Services.Contracts;
using Imgur.ViewModels.Explorer;
using Imgur.Factories;
using Imgur.ViewModels.Media;
using Imgur.Mappers;
using Windows.Media;
using Imgur.ViewModels.Account;
using Imgur.ViewModels.Tags;

namespace Imgur.Uwp
{
    /// <summary>
    ///Fornece o comportamento específico do aplicativo para complementar a classe Application padrão.
    /// </summary>
    sealed partial class App : Application
    {

        /// <summary>
        ///Instancia de Variaveis Globais do Projeto 
        /// </summary>
        private static Frame ShellFrame;
        private static Rect AppBounds;
        private IServiceProvider _serviceProvider;

        //Get App Width and Height
        public static Rect AppStartBounds => AppBounds;

        //Service Provider
        public static IServiceProvider Services
        {
            get
            {
                IServiceProvider serviceProvider = ((App)Current)._serviceProvider;

                if (serviceProvider is null)
                {
                    Debug.WriteLine("Cagada no S.P");
                }

                return serviceProvider;
            }
        }

        /// <summary>
        /// Inicializa o objeto singleton do aplicativo.  Esta é a primeira linha de código criado
        /// executado e, como tal, é o equivalente lógico de main() ou WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
        }

        /// <summary>
        /// Chamado quando o aplicativo é iniciado normalmente pelo usuário final.  Outros pontos de entrada
        /// serão usados, por exemplo, quando o aplicativo for iniciado para abrir um arquivo específico.
        /// </summary>
        /// <param name="e">Detalhes sobre a solicitação e o processo de inicialização.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            await ActivateAsync(e.PrelaunchActivated);
        }

        private async Task ActivateAsync(bool prelaunched)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

                //Inicialização do Container de Dependencia de Serviços
                _serviceProvider = configureServices();

            }

            //Recuperar Width e Height da Janela Atual
            AppBounds = ApplicationView.GetForCurrentView().VisibleBounds;

            if (prelaunched == false)
            {
                CoreApplication.EnablePrelaunch(true);

                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(ShellView));
                    ShellFrame = rootFrame;
                }

                //Evento de BackPressed para Navegação Global da RootFrame
                HardwareButtons.BackPressed += hardwareButtonsBackPressedAsync;

                //Esconder AppBar no Mobile
                await setAppBarAsync();

                //Inicializa a Janela
                Window.Current.Activate();

                //Live Tiles
                await UpdateLiveTilesInBackgroundAsync();

            }

            this.FocusVisualKind = FocusVisualKind.Reveal;


            Services.GetRequiredService<INavigator>().RootFrame = rootFrame;
        }


        private IServiceProvider configureServices()
        {

            string clientId = "b6c4abc4061d423";
            string clientSecret = "1ccc6187d2e64baaefcf49487cc1d948cfa6484e";

            DispatcherHelper.Initialize();
            var provider = new ServiceCollection()

                //Serviços Contexto UWP como Singleton (Unica Instancia)
                .AddSingleton<INavigator, Navigator>()
                .AddSingleton<ISystemInfoProvider, SystemInfoProvider>()
                .AddSingleton<ILocalSettings, LocalSettings>()
                .AddSingleton<IDispatcher, DispatcherService>()
                .AddSingleton<IMediaPlayerService, MediaPlayerService>()
                .AddSingleton<SystemMediaTransportControls>(SystemMediaTransportControls.GetForCurrentView())
                .AddSingleton<IClipboardService, ClipboardService>()
                .AddSingleton<IAppNotificationService, AppNotificationService>()
                .AddSingleton<IShareService, ShareService>()
                .AddSingleton<ILiveTilesService, LiveTilesService>()
                .AddSingleton<IDialogService, DialogService>()
                .AddSingleton<ITokenService, TokenService>()

                //Contextos como Singleton
                .AddSingleton<IUserContext, Imgur.Services.UserContext>()

                //Factories como Singleton
                .AddSingleton<IMediaVmFactory, MediaVmFactory>()
                .AddSingleton<IAccountVmFactory, AccountVmFactory>()
                .AddSingleton<IEmbedVmFactory, EmbedVmFactory>()
                .AddSingleton<ILoginInterceptorVmFactory, LoginInterceptorVmFactory>()
                .AddSingleton<IExplorerSearchVmFactory, ExplorerSearchVmFactory>()
                .AddSingleton<ITagVmFactory, TagVmFactory>()

                //ViewModels como transientes para reutilização em multiplas Views
                .AddTransient<ShellViewModel>()
                .AddTransient<SettingsViewModel>()
                .AddTransient<ShutdownViewModel>()
                .AddTransient<ExplorerViewModel>()
                .AddTransient<MediaViewModel>()
                .AddTransient<EmbedViewModel>()
                .AddTransient<AccountViewModel>()
                .AddTransient<LoginInterceptorViewModel>()
                .AddTransient<ExplorerSearchViewModel>()
                .AddTransient<TagViewModel>()

                //Services (Use Cases) como Transientes
                .AddTransient<Imgur.Services.GalleryService>()
                .AddTransient<Imgur.Services.AlbumService>()
                .AddTransient<Imgur.Services.AccountService>()
                .AddTransient<Imgur.Services.TagsService>()

                //Mappers
                .AddTransient<GalleryMapper>()
                .AddTransient<TagMapper>()
                .AddTransient<ImageMapper>()
                .AddTransient<AlbumMapper>()
                .AddTransient<AccountMapper>()

                //Imgur Api Services como Transienes
                .AddTransient<IGalleryService>(sp =>
                    new Api.Services.Actions.GalleryService(clientId)
                )
                 .AddTransient<IAlbumService>(sp =>
                    new Api.Services.Actions.AlbumService(clientId)
                )
                 .AddTransient<IAccountService>(sp =>
                    new Api.Services.Actions.AccountService(clientId)
                )
                .BuildServiceProvider(true);


            /*
            //Serviços como Singleton (Unica Instancia)
            .AddSingleton<IShareService, ShareService>()
            .AddSingleton<SystemMediaTransportControls>(SystemMediaTransportControls.GetForCurrentView())
            .AddSingleton<DataTransferManager>(DataTransferManager.GetForCurrentView())

*/

            return provider;
        }



        /// <summary>
        /// Chamado para definir as configs de AppBar no Desktop e Mobile
        /// </summary>
        private async Task setAppBarAsync()
        {
            //Set Frist Time on Desktop
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 320));
            var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonForegroundColor = Colors.White;

            // If we have a phone contract, hide the status bar
            if (Services.GetRequiredService<ISystemInfoProvider>().IsMobile())
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }
        }


        /// <summary>
        /// Chamado para definir e atualizar as Live Tiles quando o App é aberto.
        /// </summary>
        private async Task UpdateLiveTilesInBackgroundAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    // Obtém as dependências (ajuste conforme seu DI/Service Locator)
                    var liveTileService = this._serviceProvider.GetRequiredService<ILiveTilesService>();

                    await liveTileService.UpdateLiveTilesAsync();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar Live Tiles no startup: {ex.Message}");
            }
        }


        /// <summary>
        /// Chamado quando ocorre uma falha na Navegação para uma determinada página
        /// </summary>
        /// <param name="sender">O Quadro com navegação com falha</param>
        /// <param name="e">Detalhes sobre a falha na navegação</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Chamado quando a execução do aplicativo está sendo suspensa.  O estado do aplicativo é salvo
        /// sem saber se o aplicativo será encerrado ou retomado com o conteúdo
        /// da memória ainda intacto.
        /// </summary>
        /// <param name="sender">A fonte da solicitação de suspensão.</param>
        /// <param name="e">Detalhes sobre a solicitação de suspensão.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Salvar o estado do aplicativo e parar qualquer atividade em segundo plano
            deferral.Complete();
        }

        /// <summary>
        /// Chamado quando a execução do aplicativo está sendo retomado.  O estado do aplicativo é restaurado
        /// </summary>
        private void OnResuming(object sender, object e)
        {
            HardwareButtons.BackPressed += hardwareButtonsBackPressedAsync;
        }

        /// <summary>
        /// Ação quando o botão de voltar é acionado pelo Hardware no Mobile
        /// </summary>
        private async void hardwareButtonsBackPressedAsync(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            if (Services.GetRequiredService<INavigator>().CanGoBack())
            {
                Services.GetRequiredService<INavigator>().GoBack();
            }
            else
            {
                var msg = new MessageDialog("Confirm Close");
                var okBtn = new UICommand("OK");
                var cancelBtn = new UICommand("Cancel");
                msg.Commands.Add(okBtn);
                msg.Commands.Add(cancelBtn);
                IUICommand result = await msg.ShowAsync();

                if (result != null && result.Label == "OK")
                {
                    if (Services.GetRequiredService<ISystemInfoProvider>().IsMobile())
                    {
                        Services.GetRequiredService<INavigator>().RootNavigate("shutdown");
                    }
                    else
                    {
                        Current.Exit();
                    }
                        
                }
            }
        }

    }
}
