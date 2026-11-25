using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Contracts
{
    public class StoredToken
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string AccountName { get; set; }

        /// <summary>UTC unix seconds when the access token expires, if known.</summary>
        public long? ExpiresAtUnix { get; set; }
    }
}

