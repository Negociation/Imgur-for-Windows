using Imgur.Api.Services.Models.Response;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Mappers
{
    public class AlbumMapper
    {
        public Media ToMedia(AlbumResponse dto)
        {
            var media = new Media
            {
            };

            return media;
        }
    }
}
