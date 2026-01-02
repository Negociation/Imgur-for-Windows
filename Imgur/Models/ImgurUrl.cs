using Imgur.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Models
{
    public class ImgurUrl
    {
        public ImgurUrlType Type { get; set; }
        public string Id { get; set; }
        public string OriginalUrl { get; set; }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public object Data { get; set; }
    }
}
