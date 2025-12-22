using System;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Models
{
    public class Notification
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public Uri ImageUrl { get; set; }
    }
}
