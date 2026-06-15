using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Models
{
    public class User: UserAccount
    {
        public string AccessToken { get; set; }
    }
}
