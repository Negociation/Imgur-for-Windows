using Imgur.Api.Services.Models.Response;
using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Imgur.Mappers
{
    public class GalleryMapper
    {
        private readonly ILocalSettings _localSettings;
        private readonly ImageMapper _imageMapper;

        public GalleryMapper(ILocalSettings localSettings, ImageMapper imageMapper)
        {
            _localSettings = localSettings;
            _imageMapper = imageMapper;
        }

        /// <summary>
        /// Mapeia a galeria para o modelo de Lista de Media
        /// </summary>

        public List<Media> ToMediaList(List<GalleryItemResponse> dtoList)
        {
            try
            {
                if (dtoList == null)
                {
                    Debug.WriteLine("ToMediaList: dtoList veio NULL");
                    return new List<Media>();
                }

                // Mapear usando LINQ
                return dtoList.Select(item => this.ToMedia(item)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao converter GalleryItemResponse para Media:");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

                // opcional: retornar lista vazia para não quebrar a UI
                return new List<Media>();
            }
        }

        /// <summary>
        /// Mapeia um Item de Galeria para Media
        /// </summary>
        public Media ToMedia(GalleryItemResponse dto)
        {
            if (dto == null) return null;

            var media = new Media
            {
                Id = dto.id,
                Title = dto.title,
                Description = dto.description,
                DateTime = dto.datetime,
                Views = dto.Views,
                Ups = dto.Ups,
                Downs = dto.Downs,
                Votes = (dto.Ups - dto.Downs) > 0 ? dto.Ups - dto.Downs : 0,
                CoverLink = dto.Cover,
                CommentCount = dto.comment_count ?? 0,
                ImagesCount = dto.images_count,
                IsAlbum = dto.is_album,
                Link = dto.Link ?? "",
                AccountId = dto.account_url,
                MostViral = dto.in_most_viral
            };

            // Define CoverType
            if (dto.is_album && dto.Images != null && dto.Images.Any())
            {
                media.CoverType = !string.IsNullOrEmpty(dto.Images[0].Type) ? dto.Images[0].Type : "";
            }
            else
            {
                media.CoverType = !string.IsNullOrEmpty(dto.type) ? dto.type : "";
            }

            // Define CoverUri e CoverPlaceholder
            if (!string.IsNullOrEmpty(media.CoverLink))
            {
                media.CoverUri = GetCoverUri(media.CoverLink, media.CoverType);
            }
            else
            {
                media.CoverUri = dto.Link;
            }

            media.CoverPlaceholder = GetCoverPlaceholder(media.CoverLink, media.CoverType);
            media.CoverImage = media.CoverUri;
            if (media.IsAlbum)
            {
                media.Elements = new ObservableCollection<Element>(
                    _imageMapper.ToElementList(dto.Images, dto.title, dto.account_url)
                );
            }
            else
            {
                media.Elements = new ObservableCollection<Element>();
                media.Elements.Add(_imageMapper.StandaloneGalleryToElement(dto));
            }


            return media;
        }


        private string GetCoverUri(string coverLink, string coverType)
        {
            bool hdOnWifi = _localSettings.Get<bool>(LocalSettingsConstants.HDonWifi);
            string url = "https://i.imgur.com/" + coverLink;

            switch (coverType)
            {
                case "image/gif":
                    url += hdOnWifi ? "_d.jpeg?maxwidth=520&fidelity=low" : ".gif?maxwidth=500&fidelity=high";
                    break;
                case "video/mp4":
                    url += "_lq.mp4";
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

        private string GetCoverPlaceholder(string coverLink, string coverType)
        {
            string url = "https://i.imgur.com/" + coverLink;

            switch (coverType)
            {
                case "image/gif":
                case "video/mp4":
                    url += "_d.jpeg?maxwidth=520";
                    break;
                default:
                    url += "_d.jpeg?maxwidth=520&fidelity=low";
                    break;
            }

            return url;
        }


    }
}
