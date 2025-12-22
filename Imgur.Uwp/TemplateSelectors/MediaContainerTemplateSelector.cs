using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Imgur.Uwp.TemplateSelectors
{
    public class MediaContainerTemplateSelector : DataTemplateSelector{
        public DataTemplate ImageTemplate { get; set; }

        public DataTemplate GifTemplate { get; set; }

        public DataTemplate VideoTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            Media media = item as Media;
            if (media != null){
                if (media.CoverType == "image/gif"){                    
                    return GifTemplate;
                }else if (media.CoverType == "video/mp4"){
                    return VideoTemplate;
                }else{
                    return ImageTemplate;
                }
            }
            return base.SelectTemplateCore(item, container);
        }
    }
}
