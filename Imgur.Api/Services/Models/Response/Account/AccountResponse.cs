using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Api.Services.Models.Response.Account
{
    public class AccountResponse
    {

        public int id { get; set; }
        public string url { get; set; }
        public object bio { get; set; }
        public string avatar { get; set; }
        public string avatar_name { get; set; }
        public string cover { get; set; }
        public string cover_name { get; set; }
        public int reputation { get; set; }
        public string reputation_name { get; set; }
        public int created { get; set; }
        public bool pro_expiration { get; set; }
        public bool is_blocked { get; set; }

    }
}
