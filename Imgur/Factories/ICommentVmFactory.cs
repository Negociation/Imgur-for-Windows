using Imgur.Models;
using Imgur.ViewModels.Media;

namespace Imgur.Contracts
{
    public interface ICommentVmFactory
    {
        CommentViewModel GetCommentViewModel(string galleryId, Comment comment);
    }
}