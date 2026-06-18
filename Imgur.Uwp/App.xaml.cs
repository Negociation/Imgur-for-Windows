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
using Imgur.Services;
using Windows.ApplicationModel.Resources;
using Imgur.ViewModels.FileUpload;
using Imgur.Models;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Imgur.Constants;

namespace Imgur.Uwp
{
    sealed partial class App : Application
    {
        private static Frame ShellFrame;
        private static Rect AppBounds;
        private IServiceProvider _serviceProvider;

        public static Rect AppStartBounds => AppBounds;

        public static IServiceProvider Services
        {
            get
            {
                IServiceProvider serviceProvider = ((App)Current)._serviceProvider;
                if (serviceProvider is null)
                    Debug.WriteLine("Cagada no S.P");
                return serviceProvider;
            }
        }

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
        }

        // ── Entry Points ───────────────────────────────────────

        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            await ActivateAsync(e.PrelaunchActivated);
        }

        protected override async void OnShareTargetActivated(ShareTargetActivatedEventArgs e)
        {
            var shareOperation = e.ShareOperation;
            shareOperation.ReportStarted();

            try
            {
                // ← configureServices UMA vez só
                _serviceProvider = configureServices();

                var localSettings = _serviceProvider.GetRequiredService<ILocalSettings>();

                var hasCredentials =
                    !string.IsNullOrWhiteSpace(localSettings.Get<string>(LocalSettingsConstants.CustomClientId)) &&
                    !string.IsNullOrWhiteSpace(localSettings.Get<string>(LocalSettingsConstants.CustomClientSecret));



                //Auth
                var userContext = _serviceProvider.GetRequiredService<IUserContext>();
                await userContext.InitAsync();
                setTokenDelegates(userContext);
                var vm = new ShareTargetViewModel(userContext);

                var rootFrame = Window.Current.Content as Frame;
                if (rootFrame == null)
                {
                    rootFrame = new Frame();
                    Window.Current.Content = rootFrame;

                }

                HardwareButtons.BackPressed -= hardwareButtonsBackPressedAsync;

                rootFrame.Navigate(typeof(ShareTargetView));
                var shareTargetView = rootFrame.Content as ShareTargetView;
                shareTargetView.DataContext = vm;


                await setAppBarAsync();
                Window.Current.Activate();

                if (!hasCredentials)
                {
                    var toast = _serviceProvider.GetRequiredService<ISystemNotificationService>();

                    toast.ShowShareTargetBlocked();
                    await Task.Delay(200);
                    shareOperation.ReportCompleted();

                    return;
                }

                // Lê arquivos
                var sharedItems = await shareOperation.Data.GetStorageItemsAsync();
                var selectedFiles = new List<SelectedFile>();
                var allowed = new[] { "image/jpeg", "image/png", "image/gif", "video/mp4" };

                foreach (var item in sharedItems)
                {
                    if (item is StorageFile file && allowed.Contains(file.ContentType))
                        selectedFiles.Add(await FilePicker.ReadStorageFileAsync(file));
                }

                Debug.WriteLine($"[ShareTarget] Arquivos lidos: {selectedFiles.Count}");



                Debug.WriteLine($"[ShareTarget] Autenticado: {userContext.IsAuthenticated}");

                await Task.Delay(500);

                if (!userContext.IsAuthenticated)
                {
                    vm.IsInitialized = true;
                    // ShareTargetView já mostra mensagem de não logado
                    shareOperation.ReportCompleted();
                    return;
                }

                // Abre dialog
                var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
                vm.IsInitialized = true;

                Debug.WriteLine($"[ShareTarget] Abrindo dialog...");
                var result = await dialogService.ShowUploadDialogAsync(
                    selectedFiles,
                    onUploadStarted: () => shareTargetView?.ShowUploading(),
                    onUploadCompleted: async () =>
                    {
                        shareTargetView?.ShowCompleted();
                        await Task.Delay(1500); // mostra "Done!" por 1.5s
                        shareOperation.ReportCompleted();
                    });
                Debug.WriteLine($"[ShareTarget] Dialog fechou: {result}");
                shareOperation.ReportCompleted();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ShareTarget] Erro: {ex.Message}");
                shareOperation.ReportError("Erro ao processar arquivo compartilhado.");
            }
        }

        // ── Inicialização ──────────────────────────────────────

        private async Task ActivateAsync(bool prelaunched)
        {
            var rootFrame = EnsureRootFrame();

            AppBounds = ApplicationView.GetForCurrentView().VisibleBounds;

            if (prelaunched == false)
            {
                CoreApplication.EnablePrelaunch(true);

                await InitializeShellAsync(rootFrame);

                HardwareButtons.BackPressed += hardwareButtonsBackPressedAsync;

                await UpdateLiveTilesInBackgroundAsync();
            }

            this.FocusVisualKind = FocusVisualKind.Reveal;

            Services.GetRequiredService<INavigator>().RootFrame = rootFrame;
        }

        // ── Helpers de Inicialização ───────────────────────────

        private Frame EnsureRootFrame(bool isShareTarget = false)
        {
            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
                _serviceProvider = configureServices(isShareTarget);
            }
            return rootFrame;
        }

        private async Task InitializeShellAsync(Frame rootFrame)
        {
            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(ShellView));
                ShellFrame = rootFrame;

                if (rootFrame.Content is ShellView shellView)
                    shellView.Loaded += ShellView_Loaded;
            }

            await loginRetrieveAsync();
            await setAppBarAsync();

            Window.Current.Activate();

            if (!Services.GetRequiredService<ISystemInfoProvider>().IsMobile())
                ForceResourceRefresh();
        }

        private async void ShellView_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ShellView shellView)
                shellView.Loaded -= ShellView_Loaded;

            await VerifyApiStatusAsync();
            await Task.Delay(800);
            Services.GetRequiredService<IClipboardService>().StartMonitoring();
        }

        // ── DI ────────────────────────────────────────────────

        private IServiceProvider configureServices(bool isShareTarget = false)
        {
            if (!isShareTarget)
            {
                DispatcherHelper.Initialize();
            }

            var provider = new ServiceCollection()

                // Serviços UWP Singleton
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
                .AddSingleton<IImgurApiCredentialsProvider, ImgurApiCredentialsProvider>()
                .AddSingleton<IAppLifeCycleService, AppLifeCycleService>()
                .AddSingleton<IAuthBroker, AuthBroker>()
                .AddSingleton<IFilePicker, FilePicker>()
                .AddSingleton<ISystemNotificationService, SystemNotificationService>()

                // Contextos Singleton
                .AddSingleton<IUserContext, Imgur.Services.UserContext>()

                // Factories Singleton
                .AddSingleton<IMediaVmFactory, MediaVmFactory>()
                .AddSingleton<IAccountVmFactory, AccountVmFactory>()
                .AddSingleton<IEmbedVmFactory, EmbedVmFactory>()
                .AddSingleton<ILoginInterceptorVmFactory, LoginInterceptorVmFactory>()
                .AddSingleton<IExplorerSearchVmFactory, ExplorerSearchVmFactory>()
                .AddSingleton<ITagVmFactory, TagVmFactory>()
                .AddSingleton<IUploadFileVmFactory, UploadFileVmFactory>()
                .AddSingleton<IUploadInterceptorVmFactory, UploadInterceptorVmFactory>()
                .AddSingleton<ICommentVmFactory, CommentVmFactory>()

                // ViewModels Transient
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
                .AddTransient<UploadFileViewModel>()
                .AddTransient<ShareTargetViewModel>()
                .AddTransient<CommentViewModel>()

                // Services Domain Transient
                .AddTransient<Imgur.Services.GalleryService>()
                .AddTransient<Imgur.Services.AlbumService>()
                .AddTransient<Imgur.Services.AccountService>()
                .AddTransient<Imgur.Services.TagsService>()
                .AddTransient<Imgur.Services.UrlHandlerService>()
                .AddTransient<Imgur.Services.ImageUploadService>()
                .AddTransient<Imgur.Services.ImageService>()
                .AddTransient<Imgur.Services.UserMediaActionsService>()
                .AddTransient<Imgur.Services.CommentsService>()

                // Mappers Transient
                .AddTransient<GalleryMapper>()
                .AddTransient<TagMapper>()
                .AddTransient<ImageMapper>()
                .AddTransient<AlbumMapper>()
                .AddTransient<AccountMapper>()
                .AddTransient<CommentMapper>()

                // Imgur API Services Singleton (guardar delegate)
                .AddSingleton<IGalleryService>(sp =>
                    new Api.Services.Actions.GalleryService(
                        sp.GetRequiredService<IImgurApiCredentialsProvider>().ClientId))

                .AddSingleton<IAlbumService>(sp =>
                    new Api.Services.Actions.AlbumService(
                        sp.GetRequiredService<IImgurApiCredentialsProvider>().ClientId))

                .AddSingleton<IAccountService>(sp =>
                    new Api.Services.Actions.AccountService(
                        sp.GetRequiredService<IImgurApiCredentialsProvider>().ClientId))

                .AddSingleton<IApiStatusService>(sp =>
                    new Api.Services.Actions.ApiStatusService(
                        sp.GetRequiredService<IImgurApiCredentialsProvider>().ClientId))

                .AddSingleton<IAuthApiService>(sp =>
                    new Api.Services.Actions.AuthApiService(
                        sp.GetRequiredService<IImgurApiCredentialsProvider>().ClientId))

                .AddSingleton<IImageService>(sp =>
                    new Api.Services.Actions.ImageService(
                        sp.GetRequiredService<IImgurApiCredentialsProvider>().ClientId))
                .AddSingleton<ICommentService>(sp =>
                    new Api.Services.Actions.CommentService(
                        sp.GetRequiredService<IImgurApiCredentialsProvider>().ClientId))
                .AddSingleton<IMediaActionsService>(sp =>
                    new Api.Services.Actions.MediaActionsService(
                        sp.GetRequiredService<IImgurApiCredentialsProvider>().ClientId))

                .BuildServiceProvider(true);

            return provider;
        }

        // ── App Bar ────────────────────────────────────────────

        private async Task setAppBarAsync()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 320));
            var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonForegroundColor = Colors.White;

            if (Services.GetRequiredService<ISystemInfoProvider>().IsMobile())
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }
        }

        // ── Live Tiles ─────────────────────────────────────────

        private async Task UpdateLiveTilesInBackgroundAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    var liveTileService = this._serviceProvider.GetRequiredService<ILiveTilesService>();
                    await liveTileService.UpdateLiveTilesAsync();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao atualizar Live Tiles no startup: {ex.Message}");
            }
        }

        // ── Auth ───────────────────────────────────────────────

        private async Task loginRetrieveAsync()
        {
            try
            {
                var authService = this._serviceProvider.GetRequiredService<IUserContext>();
                await authService.InitAsync();
                this.setTokenDelegates(authService);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao recuperar sessão: {ex.Message}");
            }
        }

        private void setTokenDelegates(IUserContext userContext)
        {
            Debug.WriteLine($"[setTokenDelegates] IsAuthenticated: {userContext.IsAuthenticated}");
            Debug.WriteLine($"[setTokenDelegates] CurrentUser: {userContext.CurrentUser?.Username}");
            Debug.WriteLine($"[setTokenDelegates] AccessToken: {userContext.CurrentUser?.AccessToken}");

            (this._serviceProvider.GetRequiredService<IGalleryService>())
                .SetAccessTokenProvider(() => userContext.CurrentUser?.AccessToken);
            (this._serviceProvider.GetRequiredService<IAlbumService>())
                .SetAccessTokenProvider(() => userContext.CurrentUser?.AccessToken);
            (this._serviceProvider.GetRequiredService<IAccountService>())
                .SetAccessTokenProvider(() => userContext.CurrentUser?.AccessToken);
            (this._serviceProvider.GetRequiredService<IImageService>())
                .SetAccessTokenProvider(() => userContext.CurrentUser?.AccessToken);
            (this._serviceProvider.GetRequiredService<IMediaActionsService>())
                .SetAccessTokenProvider(() => userContext.CurrentUser?.AccessToken);
            (this._serviceProvider.GetRequiredService<ICommentService>())
                .SetAccessTokenProvider(() => userContext.CurrentUser?.AccessToken);
        }

        private async void ForceResourceRefresh()
        {
            try
            {
                var view = ApplicationView.GetForCurrentView();
                var size = view.VisibleBounds;
                view.TryResizeView(new Size(size.Width + 1, size.Height + 1));
                await Task.Delay(50);
                view.TryResizeView(new Size(size.Width, size.Height));
            }
            catch { }
        }

        private async Task VerifyApiStatusAsync()
        {
            try
            {
                var apiStatusService = Services.GetRequiredService<IApiStatusService>();
                var status = await apiStatusService.GetApiStatusAsync();

                if (status?.Success == true && status?.Data.status != "operational")
                {
                    await Task.Delay(2000);
                    var appNotificationService = Services.GetRequiredService<IAppNotificationService>();
                    appNotificationService.AddApiWarningNotification(status?.Data.status);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao verificar status da API no startup: {ex.Message}");
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            HardwareButtons.BackPressed -= hardwareButtonsBackPressedAsync;
            deferral.Complete();
        }

        private void OnResuming(object sender, object e)
        {
            HardwareButtons.BackPressed += hardwareButtonsBackPressedAsync;
        }

        private async void hardwareButtonsBackPressedAsync(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            if (Services.GetRequiredService<INavigator>().CanGoBack())
            {
                Services.GetRequiredService<INavigator>().GoBack();
            }
            else
            {
                var resourceLoader = ResourceLoader.GetForCurrentView();

                var closeTitle = resourceLoader.GetString("AppCloseTitle") ?? "Leaving Already ?";
                var closeContent = resourceLoader.GetString("AppCloseContent") ?? "Are you sure you want to close the app ?";
                var confirmLabel = resourceLoader.GetString("AppCloseConfirm") ?? "Yes";
                var cancelLabel = resourceLoader.GetString("AppCloseCancel") ?? "Cancel";

                var msg = new MessageDialog(closeContent, closeTitle);
                var okBtn = new UICommand(confirmLabel);
                var cancelBtn = new UICommand(cancelLabel);
                msg.Commands.Add(okBtn);
                msg.Commands.Add(cancelBtn);

                IUICommand result = await msg.ShowAsync();

                if (result != null && result.Label == confirmLabel)
                {
                    if (Services.GetRequiredService<ISystemInfoProvider>().IsMobile())
                        Services.GetRequiredService<INavigator>().RootNavigate("shutdown");
                    else
                        Current.Exit();
                }
            }
        }
    }
}