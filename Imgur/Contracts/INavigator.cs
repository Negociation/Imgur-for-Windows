using System;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface INavigator
    {

        event EventHandler<string> NavigateInvoked;

        event EventHandler<bool> FullScreenModeChanged;

        /// <summary>
        /// Fired whenever a fullscreen view reports its current scroll offset.
        /// Listeners (e.g. ShellView) can use this to adjust UI opacity.
        /// </summary>
        event EventHandler<double> ScrollOffsetChanged;

        /// <summary>
        /// Called from a fullscreen view code-behind to broadcast the current
        /// vertical scroll offset to any registered listener.
        /// </summary>
        void ReportScrollOffset(double offset);

        object RootFrame { get; set; }

        /// <summary>
        /// The inner frame that can navigate. This must be set before
        /// any method is called.
        /// </summary>
        object Frame { get; set; }

        /// <summary>
        /// True when the app was terminated and a navigation journal should be restored into MainFrame.
        /// </summary>
        bool PendingNavigationRestore { get; set; }

        /// <summary>
        /// Navigates to a frame Page inside the Shell.
        /// </summary>
        void Navigate(string name, object vm = null);


        /// <summary>
        /// Navigates to a outside shell page.
        /// </summary>
        void RootNavigate(string name);


        bool CanGoBack();

        void Close();

        /// <summary>
        /// Attempts to navigate back.
        /// </summary>
        /// <param name="sourcePage">Optional. If provided,
        /// then specific go back functionality may be applied.
        /// For example, if ScreensaverPage is provided, the 
        /// RootFrame is used for GoBack.</param>
        void GoBack(string sourcePage = null);


        void NavigateMediaToFullScreen(object payload = null);

        /// <summary>
        /// Persists the inner frame back stack so it can be restored after terminate.
        /// </summary>
        Task SaveNavigationStateAsync();

        /// <summary>
        /// Checks local journal and sets <see cref="PendingNavigationRestore"/> when Terminated.
        /// </summary>
        Task PrepareNavigationRestoreAsync();

        /// <summary>
        /// Drops any persisted navigation journal (fresh launch / closed by user).
        /// </summary>
        Task ClearNavigationStateAsync();

        /// <summary>
        /// Restores the inner frame from the last saved journal. Returns false if nothing was restored.
        /// </summary>
        bool TryRestoreNavigationState();

    }
}
