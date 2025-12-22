using Imgur.Models;
using Imgur.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Factories
{
    public interface IAccountVmFactory
    {
        AccountViewModel GetAccountViewModel(UserAccount m);
    }
}
