using Imgur.Api.Services.Contracts;
using Imgur.Enums;
using Imgur.Mappers;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Services
{
    public class ImageService
    {
        private readonly IImageService _apiService;
        private readonly ImageMapper _imageMapper;

        public ImageService(IImageService apiService, ImageMapper imageMapper)
        {
            _apiService = apiService;
            _imageMapper = imageMapper;
        }

        public async Task<Result<Media>> GetImageById(string id)
        {
            var item = await _apiService.GetImageAsync(id);
            if (!item.Success)
                return Result<Media>.Failure(item.Status.ToString(), ErrorType.Server);

            return Result<Media>.Success(_imageMapper.ToMedia(item.Data));
        }
    }
}
