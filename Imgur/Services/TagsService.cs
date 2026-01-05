using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response;
using Imgur.Enums;
using Imgur.Mappers;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Services
{
    public class TagsService
    {

        private IGalleryService _apiService;
        private TagMapper _tagMapper;

        public TagsService(IGalleryService galleryService, TagMapper tagMapper)
        {
            _apiService = galleryService;
            _tagMapper = tagMapper;
        }

        public async Task<Result<Tag>> GetStaffPicks()
        {
            ApiResponse<TagResponse> item = null;
            item = await _apiService.GetTagAsync(
                "staff_picks"
                );

            if (!item.Success)
            {
                return Result<Tag>.Failure(item.Status.ToString(), ErrorType.Server);

            }

            //Mapear Usando Linq
            var tagMapped = _tagMapper.ToTag(item.Data);

            return Result<Tag>.Success(tagMapped);
        }

        public async Task<Result<IReadOnlyList<Tag>>> SearchTags(string query, int page = 0)
        {
            ApiResponse<List<TagResponse>> items = null;
            items = await _apiService.TagSearchAsync(query);

            if (!items.Success)
            {
                return Result<IReadOnlyList<Tag>>.Failure(items.Status.ToString(), ErrorType.Server);

            }

            var tagListMapped = _tagMapper.ToTagList(items.Data);
            return Result<IReadOnlyList<Tag>>.Success(tagListMapped);
        }

        public async Task<Result<Tag>> GetTagById(string id)
        {
            ApiResponse<TagResponse> item = null;
            item = await _apiService.GetTagAsync(
                id
                );

            if (!item.Success)
            {
                return Result<Tag>.Failure(item.Status.ToString(), ErrorType.Server);

            }

            //Mapear Usando Linq
            var tagMapped = _tagMapper.ToTag(item.Data);

            return Result<Tag>.Success(tagMapped);
        }
    }
}
