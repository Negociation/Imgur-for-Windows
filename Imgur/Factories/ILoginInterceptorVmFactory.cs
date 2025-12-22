using Imgur.Enums;
using Imgur.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Factories
{
    public interface ILoginInterceptorVmFactory
    {

        LoginInterceptorViewModel getLoginInterceptorViewModel(LoginInterceptorEnum messageType);
    }
}
