using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public interface ILiveTilesService
    {

        Task UpdateLiveTilesAsync();

        Task OnLiveTilesConfigChangedAsync();
    }
}
