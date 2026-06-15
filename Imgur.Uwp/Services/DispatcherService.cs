using Imgur.Contracts;
using System;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

namespace Imgur.Uwp.Services
{
    public class DispatcherService : IDispatcher
    {
        public void CheckBeginInvokeOnUi(Action action)
        {
            try
            {
                var dispatcher = CoreWindow.GetForCurrentThread()?.Dispatcher
                              ?? CoreApplication.MainView.CoreWindow.Dispatcher;

                if (dispatcher.HasThreadAccess)
                    action();
                else
                    _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
            }
            catch
            {
                // fallback — executa direto se não conseguir dispatcher
                action();
            }
        }
    }
}