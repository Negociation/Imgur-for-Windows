using Imgur.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Imgur.Models
{
    public class Tag : Observable
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string Background { get; set; }
        public string BackgroundTile { get; set; }

        public long ItemsCount { get; set; }

        public ObservableCollection<Media> Items { get; set; }

    }
}
