using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Api.Services.Models.Response.Suggest
{
    public class UsersResponse
    {
        public long id { get; set; }
        public string url { get; set; }
        public string reputation_name { get; set; }
    }
}
