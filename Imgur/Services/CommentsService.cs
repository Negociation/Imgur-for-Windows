using Imgur.Api.Services.Contracts;
using Imgur.Enums;
using Imgur.Mappers;
using Imgur.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgur.Services
{
    public class CommentsService
    {
        private readonly ICommentService _apiService;
        private readonly CommentMapper _mapper;

        public CommentsService(ICommentService apiService, CommentMapper mapper)
        {
            _apiService = apiService;
            _mapper = mapper;
        }

        public async Task<Result<List<Comment>>> GetCommentsAsync(
            string galleryId, string sort = "best")
        {
            var response = await _apiService.GetGalleryCommentsAsync(galleryId, sort);
            if (!response.Success)
                return Result<List<Comment>>.Failure(response.Status.ToString(), ErrorType.Server);

            return Result<List<Comment>>.Success(_mapper.ToCommentList(response.Data));
        }

        public async Task<Result<bool>> PostCommentAsync(string galleryId, string body)
        {
            var response = await _apiService.PostCommentAsync(galleryId, body);
            if (!response.Success)
                return Result<bool>.Failure(response.Status.ToString(), ErrorType.Server);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ReplyAsync(long parentId, string galleryId, string body)
        {
            var response = await _apiService.ReplyCommentAsync(parentId, galleryId, body);
            if (!response.Success)
                return Result<bool>.Failure(response.Status.ToString(), ErrorType.Server);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteAsync(long commentId)
        {
            var response = await _apiService.DeleteCommentAsync(commentId);
            if (!response.Success)
                return Result<bool>.Failure(response.Status.ToString(), ErrorType.Server);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> VoteAsync(long commentId, string vote)
        {
            var response = await _apiService.VoteCommentAsync(commentId, vote);
            if (!response.Success)
                return Result<bool>.Failure(response.Status.ToString(), ErrorType.Server);

            return Result<bool>.Success(true);
        }
    }
}