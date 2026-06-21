using System;

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


        void NavigateMediaToFullScreen();

    }
}
