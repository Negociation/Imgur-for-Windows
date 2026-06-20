using Imgur.ViewModels.Account;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Imgur.Uwp.Controls
{
    public sealed partial class AccountTileControl : UserControl
    {
        public AccountTileControl()
        {
            this.InitializeComponent();

            Window.Current.SizeChanged += AccountTileControl_SizeChanged;
            Loaded += AccountTileControl_Loaded;
        }

        private void AccountTileControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState();
        }

        private void AccountTileControl_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (!EnableCompactMode)
            {
                VisualStateManager.GoToState(this, "TileNormal", false);
                return;
            }

            double width = Window.Current.Bounds.Width;

            var state = width < 500 ? "TileCompact" : "TileNormal";

            Debug.WriteLine($"[AccountTile] Applying state: {state} (width={width})");

            bool result = VisualStateManager.GoToState(this, state, true);

            Debug.WriteLine($"[AccountTile] GoToState result = {result}");
        }

        #region ViewModel

        public AccountViewModel ViewModel
        {
            get => (AccountViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(AccountViewModel),
                typeof(AccountTileControl),
                new PropertyMetadata(null));

        #endregion

        #region EnableCompactMode

        public bool EnableCompactMode
        {
            get => (bool)GetValue(EnableCompactModeProperty);
            set => SetValue(EnableCompactModeProperty, value);
        }

        public static readonly DependencyProperty EnableCompactModeProperty =
            DependencyProperty.Register(
                nameof(EnableCompactMode),
                typeof(bool),
                typeof(AccountTileControl),
                new PropertyMetadata(false));

        #endregion
    }
}