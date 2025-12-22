using Imgur.Api.Services.Models.Enum;
using Imgur.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Models
{
    public class Section : Observable
    {
        public GallerySection section { get; set; }
        public List<Sort> sorts { get; set; }

        public Section(GallerySection section, List<Sort> sorts)
        {
            this.section = section;
            this.sorts = sorts;
        }
    }
}
