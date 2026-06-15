using Imgur.Api.Services.Contracts;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Services
{
    public class UserMediaActionsService
    {
        private readonly IMediaActionsService _apiService;

        public UserMediaActionsService(IMediaActionsService apiService)
        {
            _apiService = apiService;
        }

        public async Task<Result<bool>> VoteAsync(string id, string vote)
        {
            var response = await _apiService.VoteAsync(id, vote);
            if (!response.Success)
                return Result<bool>.Failure($"Erro ao votar: {response.Status}");

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> FavoriteAsync(string id, bool isAlbum)
        {
            var response = isAlbum
                ? await _apiService.FavoriteAlbumAsync(id)
                : await _apiService.FavoriteImageAsync(id);

            if (!response.Success)
                return Result<bool>.Failure($"Erro ao favoritar: {response.Status}");

            return Result<bool>.Success(true);
        }
    }
}

