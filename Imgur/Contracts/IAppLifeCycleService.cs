using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface IAppLifeCycleService
    {
        Task RestartAsync(string reason = null);

        Task RestartToApplyCustomApiSettings();
    }
}
