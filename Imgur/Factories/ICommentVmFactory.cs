using Imgur.Models;
using Imgur.ViewModels.Media;

namespace Imgur.Contracts
{
    public interface ICommentVmFactory
    {
        CommentViewModel GetMediaCommentViewModel(string galleryId, Comment comment);
        CommentViewModel GetCommentViewModel(Comment comment);
    }
}