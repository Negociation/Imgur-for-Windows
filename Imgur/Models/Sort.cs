using Imgur.Api.Services.Models.Enum;
using Imgur.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
namespace Imgur.Models
{
    public class Sort : Observable
    {
        public object sort { get; set; }

        public Sort(GallerySort sort)
        {
            this.sort = sort;
        }

        public Sort(GalleryWindow windowSort)
        {
            this.sort = windowSort;
        }
    }
}
