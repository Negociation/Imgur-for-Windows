using GalaSoft.MvvmLight.Threading;
using Imgur.Contracts;
using System;

namespace Imgur.Uwp.Services
{
    public class DispatcherService: IDispatcher
    {

        public void CheckBeginInvokeOnUi(Action action)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(action);
        }
    }
}
