using System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Imgur.Models
{
    public class NotificationViewModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string ImageUrl { get; set; }
        public string Extra { get; set; }

        public ICommand ActionCommand { get; set; }
    }
}
