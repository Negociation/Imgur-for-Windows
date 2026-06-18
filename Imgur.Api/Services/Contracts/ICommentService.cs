using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response.Comment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Contracts
{
    public interface ICommentService: IApiService
    {
        Task<ApiResponse<List<CommentResponse>>> GetGalleryCommentsAsync(string galleryId, string sort = "best");
        Task<ApiResponse<CommentResponse>> GetCommentAsync(long commentId);
        Task<ApiResponse<CommentResponse>> PostCommentAsync(string galleryId, string body);
        Task<ApiResponse<CommentResponse>> ReplyCommentAsync(long parentId, string galleryId, string body);
        Task<ApiResponse<bool>> DeleteCommentAsync(long commentId);
        Task<ApiResponse<string>> VoteCommentAsync(long commentId, string vote);
    }
}