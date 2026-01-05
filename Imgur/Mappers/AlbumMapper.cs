using Imgur.Api.Services.Models.Response;
using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Imgur.Mappers
{
    public class AlbumMapper
    {
        private readonly ILocalSettings _localSettings;
        private readonly ImageMapper _imageMapper;

        public AlbumMapper(ILocalSettings localSettings, ImageMapper imageMapper)
        {
            _localSettings = localSettings;
            _imageMapper = imageMapper;
        }

        public Media ToMedia(AlbumResponse dto)
        {
            if (dto == null) return null;

            var media = new Media
            {
                Id = dto.id,
                Title = dto.title,
                Description = dto.description,
                DateTime = dto.datetime,
                Views = dto.views,
                Ups = 0,
                Downs = 0,
                Votes = 0,
                CoverLink = "",
                CommentCount = 0,
                ImagesCount = dto.images_count,
                IsAlbum = true,
                Link = dto.link ?? "",
                AccountId = dto.account_url,
                IsBasicAlbum = true,
            };

            if(dto.cover == "")
            {
                var first = dto.images.FirstOrDefault();

                if (first == null)
                {
                    media.CoverPlaceholder = "0";
                }

                media.CoverPlaceholder = GetCoverPlaceholder(first.Id, first.Type);
            }

            if (media.IsAlbum)
            {
                media.Elements = new ObservableCollection<Element>(
                    _imageMapper.ToElementList(dto.images, dto.title, dto.account_url)
                );
            }
            else
            {
                media.Elements = new ObservableCollection<Element>();
            }

            return media;
        }

        private string GetCoverPlaceholder(string coverLink, string coverType)
        {
            bool hdOnWifi = _localSettings.Get<bool>(LocalSettingsConstants.HDonWifi);
            string url = "https://i.imgur.com/" + coverLink;

            switch (coverType)
            {
                case "image/gif":
                    url += hdOnWifi ? "_d.jpeg?maxwidth=520&fidelity=low" : ".gif?maxwidth=500&fidelity=high";
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
    }
}
