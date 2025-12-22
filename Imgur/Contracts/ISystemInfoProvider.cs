using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Contracts
{
    public interface ISystemInfoProvider
    {
        bool IsFirstRun();

        bool IsMobile();

        bool IsContinuum();

        bool IsXbox();

        bool IsMinBuild(int build);
    }
}
