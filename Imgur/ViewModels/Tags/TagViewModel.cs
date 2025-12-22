using Imgur.Factories;
using Imgur.Helpers;
using Imgur.Models;
using Imgur.ViewModels.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Imgur.ViewModels.Tags
{
    public class TagViewModel : Observable
    {

        //-- Current Tag to Show
        private Tag _currentTag;

        //-- Listagem de Itens (ViewModels)
        public ObservableCollection<MediaViewModel> RetrievedMediaVmCollection { get; } = new ObservableCollection<MediaViewModel>();

        public Tag CurrentTag
        {
            get { return _currentTag; }
            set
            {
                _currentTag = value;
                OnPropertyChanged("CurrentTag");
            }
        }


        public TagViewModel(Tag tag, IMediaVmFactory _mediaVmFactory)
        {
            CurrentTag = tag;

            if (tag.Items != null)
            {
                foreach (var media in tag.Items)
                    RetrievedMediaVmCollection.Add(_mediaVmFactory.GetMediaViewModel(media));
            }
        }
    }
}
