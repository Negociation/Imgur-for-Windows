using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Api.Services.Contracts
{
    public interface IApiService
    {
        void SetAccessTokenProvider(Func<string> provider);

    }
}
