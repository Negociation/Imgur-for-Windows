using Imgur.Api.Services.Models.Response;
using Imgur.Api.Services.Models.Response.Image;
using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Imgur.Mappers
{
    public class ImageMapper
    {
        private readonly ILocalSettings _localSettings;

        public ImageMapper(ILocalSettings localSettings)
        {
            _localSettings = localSettings;
        }

        public Media ToMedia(ImageResponse dto)
        {
            if (dto == null) return null;
            Debug.WriteLine($"[ImageMapper] {Newtonsoft.Json.JsonConvert.SerializeObject(dto, Newtonsoft.Json.Formatting.Indented)}");

            var media = new Media
            {
                Id = dto.Id,
                Title = dto.Title ?? dto.Id,
                Description = dto.Description,
                DateTime = dto.DateTime,
                Views = dto.Views,
                Ups = 0,
                Downs = 0,
                Votes = 0,
                CommentCount = 0,
                ImagesCount = 1,
                IsAlbum = false,
                IsBasicAlbum = true,
                Link = dto.Link ?? "",
                AccountId = dto.AccountUrl,
                Elements = new ObservableCollection<Element>(ToElementList(new List<ImageResponse> { dto }, dto.Title, dto.AccountUrl))
            };

            media.CoverPlaceholder = GetCoverPlaceholder(dto.Id, dto.Type, dto.Mp4);

            return media;
        }

        private string GetCoverPlaceholder(string id, string type, string mp4 = null)
        {
            bool hdOnWifi = _localSettings.Get<bool>(LocalSettingsConstants.HDonWifi);
            string url = "https://i.imgur.com/" + id;

            switch (type)
            {
                case "image/gif":
                    url += hdOnWifi
                        ? "_d.jpeg?maxwidth=520&fidelity=low"
                        : ".gif?maxwidth=500&fidelity=high";
                    break;
                case "video/mp4":
                    url += "_d.jpeg?maxwidth=520";
                    break;
                default:
                    string ext = hdOnWifi ? "jpeg" : "gif";
                    string maxWidth = hdOnWifi ? "400" : "520";
                    string fidelity = hdOnWifi ? "low" : "high";
                    url += "_d." + ext + "?maxwidth=" + maxWidth + "&fidelity=" + fidelity;
                    break;
            }

            return url;
        }

        public List<Element> ToElementList(List<ImageResponse> dtoList, string mediaTitle = null, string mediaAuthor = null)
        {
            if (dtoList == null)
            {
                Debug.WriteLine("ToElementList: dtoList veio NULL");
                return new List<Element>();
            }

            return dtoList.Select(item => this.ToElement(item, mediaTitle, mediaAuthor)).ToList();

        }
        public Element ToElement(ImageResponse dto, string mediaTitle = null, string mediaAuthor = null)
        {
            var element = new Element
            {
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
