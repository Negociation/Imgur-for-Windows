using System;
using System.Collections.Generic;
using System.Text;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.ViewModels.Account;
using Microsoft.Extensions.DependencyInjection;

namespace Imgur.Factories
{
    public class LoginInterceptorVmFactory : ILoginInterceptorVmFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public LoginInterceptorVmFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public LoginInterceptorViewModel getLoginInterceptorViewModel(LoginInterceptorEnum messageType)
        {
            var navigator = _serviceProvider.GetRequiredService<INavigator>();

            return new LoginInterceptorViewModel(messageType, navigator);
        }
    }
}
