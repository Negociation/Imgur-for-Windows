using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Models
{
    public class UserAccount
    {
        public string Username { get; set; }

        public string Avatar { get; set; }

        public string Cover { get; set; }

        public int Reputation { get; set; }
        public string ReputationName { get; set; }
    }
}
