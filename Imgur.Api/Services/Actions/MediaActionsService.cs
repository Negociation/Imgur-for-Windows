using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Actions
{
    public class MediaActionsService : ApiService, IMediaActionsService
    {
        public MediaActionsService(string clientId) : base(clientId) { }

        public async Task<ApiResponse<string>> UpvoteAsync(string galleryId)
        {
            Debug.WriteLine($"[MediaActionsService] Upvote: {galleryId}");
            return await PostAsync<string>($"gallery/{galleryId}/vote/up");
        }

        public async Task<ApiResponse<string>> DownvoteAsync(string galleryId)
        {
            Debug.WriteLine($"[MediaActionsService] Downvote: {galleryId}");
            return await PostAsync<string>($"gallery/{galleryId}/vote/down");
        }

        public async Task<ApiResponse<string>> VoteAsync(string galleryId, string vote)
        {
            Debug.WriteLine($"[MediaActionsService] Vote {vote}: {galleryId}");
            return await PostAsync<string>($"gallery/{galleryId}/vote/{vote}");
        }

        public async Task<ApiResponse<string>> FavoriteImageAsync(string imageId)
        {
            Debug.WriteLine($"[MediaActionsService] FavoriteImage: {imageId}");
            return await PostAsync<string>($"image/{imageId}/favorite");
        }

        public async Task<ApiResponse<string>> FavoriteAlbumAsync(string albumId)
        {
            Debug.WriteLine($"[MediaActionsService] FavoriteAlbum: {albumId}");
            return await PostAsync<string>($"album/{albumId}/favorite");
        }
    }
}
