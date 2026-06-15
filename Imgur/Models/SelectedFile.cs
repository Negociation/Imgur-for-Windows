using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Models
{
    public class SelectedFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string MimeType { get; set; }
        public object NativeFile { get; set; }
        public byte[] Bytes { get; set; }
    }
}
