using Imgur.Api.Services.Models.Response;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
                Description = dto.description,
                Background = $"https://i.imgur.com/{dto.background_hash}_d.png?maxwidth=2000",
                BackgroundTile = $"https://i.imgur.com/{dto.background_hash}_d.png?maxwidth=300",

                ItemsCount = dto.total_items
            };

            //tag.Items =  new ObservableCollection<Media>();
            tag.Items = new ObservableCollection<Media>(_galleryMapper.ToMediaList(dto.items)); 
            return tag;
        }


        public List<Tag> ToTagList(List<TagResponse> dtoList)
        {
            try{
                if (dtoList == null)
                {
                    Debug.WriteLine("ToMediaList: dtoList veio NULL");
                    return new List<Tag>();
                }

                // Mapear usando LINQ
                return dtoList.Select(item => this.ToTag(item)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao converter TagResponse para Tag:");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

                // opcional: retornar lista vazia para não quebrar a UI
                return new List<Tag>();
            }
        }
    }
}
