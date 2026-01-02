using Imgur.Api.Services.Models.Response;
using Imgur.Api.Services.Models.Response.Image;
using Imgur.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Imgur.Mappers
{
    public class ImageMapper
    {

        public List<Element> ToElementList(List<ImageResponse> dtoList, string mediaTitle = null, string mediaAuthor = null)
        {
            if (dtoList == null)
            {
                Debug.WriteLine("ToElementList: dtoList veio NULL");
                return new List<Element>();
            }

            return dtoList.Select(item => this.ToElement(item,mediaTitle,mediaAuthor)).ToList();

        }
        public Element ToElement(ImageResponse dto, string mediaTitle = null, string mediaAuthor = null)
        {
            var element = new Element {
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type,
                Link = dto.Link ?? "",
                Width = dto.Width,
                Height = dto.Height,
                MediaTitle = (dto.Title ?? mediaTitle) ?? "Imgur",
                MediaAuthor = (mediaAuthor ?? "--")
            };

            return element;
        }

        public Element StandaloneGalleryToElement(GalleryItemResponse dto)
        {
            var element = new Element
            {
                Title = dto.title,
                Description = dto.description,
                Type = dto.type,
                Link = dto.Link,
                Width = dto.Width,
                Height = dto.Height,
                MediaTitle = dto.title ?? "Imgur",
                MediaAuthor = dto.account_url ?? "--",
            };

            return element;
        }
    }
}
