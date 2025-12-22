using Imgur.Api.Services.Contracts;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Mappers;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Services
{
    public class AlbumService
    {

        private ILocalSettings _localSettings;
        private IAlbumService _apiService;
        private AlbumMapper _albumMapper;

        public AlbumService(
           ILocalSettings localSettings,
           IAlbumService apiService,
           AlbumMapper albumMapper
           )
        {
            _localSettings = localSettings;
            _apiService = apiService;
            _albumMapper = albumMapper;
        }

        public async Task<Result<Media>> GetAlbumById(string id)
        {
            var item = await _apiService.GetAlbumAsync(id);

            if (!item.Success)
            {
                return Result<Media>.Failure(item.Status.ToString(), ErrorType.Server);
            }

            return Result<Media>.Success(this._albumMapper.ToMedia(item.Data));
        }
    }
}
