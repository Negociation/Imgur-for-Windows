using Imgur.Helpers;
using System;
using System.Collections.ObjectModel;

namespace Imgur.Models
{
    public class Comment : Observable
    {
        public long Id { get; set; }
        public string ImageId { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string AuthorAvatar { get; set; }
        public long AuthorId { get; set; }
        public long DateTime { get; set; }
        public long ParentId { get; set; }
        public bool Deleted { get; set; }
        public string Platform { get; set; }
        public bool IsRootComment { get; set; }

        private int _ups;
        public int Ups
        {
            get => _ups;
            set { _ups = value; OnPropertyChanged(nameof(Ups)); OnPropertyChanged(nameof(Points)); }
        }

        private int _downs;
        public int Downs
        {
            get => _downs;
            set { _downs = value; OnPropertyChanged(nameof(Downs)); OnPropertyChanged(nameof(Points)); }
        }

        public int Points => Ups - Downs;

        private string _vote;
        public string Vote
        {
            get => _vote;
            set { _vote = value; OnPropertyChanged(nameof(Vote)); OnPropertyChanged(nameof(IsUpvoted)); OnPropertyChanged(nameof(IsDownvoted)); }
        }

        public bool IsUpvoted => Vote == "up";
        public bool IsDownvoted => Vote == "down";

        public bool HasReplies => Children != null && Children.Count > 0;
        public int ReplyCount => Children?.Count ?? 0;

        public string ShortDate
        {
            get
            {
                var dt = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddSeconds(DateTime);
                var diff = System.DateTime.UtcNow - dt;
                if (diff.TotalDays >= 1) return $"{(int)diff.TotalDays}d";
                if (diff.TotalHours >= 1) return $"{(int)diff.TotalHours}h";
                return $"{(int)diff.TotalMinutes}m";
            }
        }

        public ObservableCollection<Comment> Children { get; set; }
            = new ObservableCollection<Comment>();
    }
}