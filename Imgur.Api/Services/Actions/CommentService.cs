using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response.Comment;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Actions
{
    public class CommentService : ApiService, ICommentService
    {
        public CommentService(string clientId) : base(clientId) { }
        public async Task<ApiResponse<List<CommentResponse>>> GetGalleryCommentsAsync(
            string galleryId, string sort = "best")
        {
            Debug.WriteLine($"[CommentService] GetComments: {galleryId}");

            var result = await GetAsync<List<CommentResponse>>($"gallery/{galleryId}/comments/{sort}");

            return result;
        }

        public async Task<ApiResponse<CommentResponse>> GetCommentAsync(long commentId)
        {
            Debug.WriteLine($"[CommentService] GetComment: {commentId}");
            return await GetAsync<CommentResponse>($"comment/{commentId}");
        }

        public async Task<ApiResponse<CommentResponse>> PostCommentAsync(string galleryId, string body)
        {
            Debug.WriteLine($"[CommentService] PostComment: {galleryId}");
            var content = new FormUrlEncodedContent(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>("image_id", galleryId),
                new System.Collections.Generic.KeyValuePair<string, string>("comment",  body)
            });
            return await PostAsync<CommentResponse>($"comment", content);
        }

        public async Task<ApiResponse<CommentResponse>> ReplyCommentAsync(
            long parentId, string galleryId, string body)
        {
            Debug.WriteLine($"[CommentService] ReplyComment: {parentId}");
            var content = new FormUrlEncodedContent(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>("image_id", galleryId),
                new System.Collections.Generic.KeyValuePair<string, string>("comment",  body)
            });
            return await PostAsync<CommentResponse>($"comment/{parentId}", content);
        }

        public async Task<ApiResponse<bool>> DeleteCommentAsync(long commentId)
        {
            Debug.WriteLine($"[CommentService] DeleteComment: {commentId}");
            return await DeleteAsync<bool>($"comment/{commentId}");
        }

        public async Task<ApiResponse<string>> VoteCommentAsync(long commentId, string vote)
        {
            Debug.WriteLine($"[CommentService] VoteComment: {commentId} → {vote}");
            return await PostAsync<string>($"comment/{commentId}/vote/{vote}");
        }
    }
}