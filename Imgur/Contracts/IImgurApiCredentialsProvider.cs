using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Contracts
{
    public interface IImgurApiCredentialsProvider
    {
        string ClientId { get; }
        string ClientSecret { get; }
    }
}
