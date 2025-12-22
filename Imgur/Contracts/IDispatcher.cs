using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Contracts
{
    public interface IDispatcher
    {
        void CheckBeginInvokeOnUi(Action action);

    }
}
