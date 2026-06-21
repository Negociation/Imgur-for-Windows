using Imgur.Api.Services.Models.Response.Comment;
using Imgur.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Imgur.Mappers
{
    public class CommentMapper
    {
        public Comment ToComment(CommentResponse dto, bool isRoot = true)
        {
            if (dto == null) return null;

            var comment = new Comment
            {
                Id = dto.Id,
                ImageId = dto.ImageId,
                Cover = $"https://i.imgur.com/{dto.AlbumCover}b.png",
                Body = System.Net.WebUtility.HtmlDecode(dto.Body),
                Author = dto.Author,
                AuthorId = dto.AuthorId,
                DateTime = dto.DateTime,
                ParentId = dto.ParentId,
                Deleted = dto.Deleted,
                Platform = dto.Platform,
                Ups = dto.Ups,
                Downs = dto.Downs,
                Vote = dto.Vote,
                IsRootComment = isRoot,
                AuthorAvatar = $"https://imgur.com/user/{dto.Author}/avatar"
            };

            // Mapeia replies recursivamente
            if (dto.Children != null && dto.Children.Any())
            {
                comment.Children = new ObservableCollection<Comment>(
                    dto.Children.Select(c => ToComment(c, false))
                );
            }

            return comment;
        }

        public List<Comment> ToCommentList(List<CommentResponse> dtos)
        {
            if (dtos == null) return new List<Comment>();
            return dtos.Select(child => ToComment(child)).ToList();
        }
    }
}