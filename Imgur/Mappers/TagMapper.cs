using Imgur.Api.Services.Models.Response;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Imgur.Mappers
{
    public class TagMapper
    {

        private GalleryMapper _galleryMapper;

        public TagMapper(GalleryMapper galleryMapper)
        {
            _galleryMapper = galleryMapper;
        }

        public Tag ToTag(TagResponse dto)
        {
            var tag = new Tag
            {
                Id = dto.name,
                Title = dto.display_name,
                Description = dto.description
            };

            //tag.Items =  new ObservableCollection<Media>();
            tag.Items = new ObservableCollection<Media>(_galleryMapper.ToMediaList(dto.items)); 
            return tag;
        }
    }
}
