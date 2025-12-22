using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Enum;
using Imgur.Api.Services.Models.Response;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Mappers;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Services
{
    public class GalleryService
    {
        private ILocalSettings _localSettings;
        private IGalleryService _apiService;

        private readonly GalleryMapper _galleryMapper;
        private readonly AlbumMapper _albumMapper;
        public GalleryService(
            ILocalSettings localSettings,
            IGalleryService apiService,
            GalleryMapper galleryMapper,
            AlbumMapper albumMapper
            )
        {
            _localSettings = localSettings;
            _apiService = apiService;
            _galleryMapper = galleryMapper;
            _albumMapper = albumMapper;
        }

        public async Task<Result<IReadOnlyList<Media>>> GetExplorerMedia(GallerySection sectionElement, object sortOrWindowElement, int currentPage = 0)
        {
            ApiResponse<List<GalleryItemResponse>> items = null;

            if (sortOrWindowElement is GalleryWindow windowElement)
            {
                items = await _apiService.GetGalleryAsync(
                    section: sectionElement,
                    sort: GallerySort.Viral, // sort padrão quando usa Window
                    window: windowElement,
                    page: currentPage
                );
            }
            // Verifica se é GallerySort
            else if (sortOrWindowElement is GallerySort sortElement)
            {
                items = await _apiService.GetGalleryAsync(
                    section: sectionElement,
                    sort: sortElement,
                    window: GalleryWindow.Day, // window padrão quando usa Sort
                    page: currentPage
                );
            }



            if (!items.Success)
            {
                return Result<IReadOnlyList<Media>>.Failure(items.Status.ToString(), ErrorType.Server);
            }

            //Mapear Usando Linq
            var explorerListMapped = _galleryMapper.ToMediaList(items.Data);

            return Result<IReadOnlyList<Media>>.Success(explorerListMapped);
        }

        public async Task<Result<IReadOnlyList<Media>>> GetRandomMedia(int page = 0)
        {
            ApiResponse<List<GalleryItemResponse>> items = null;

            items = await _apiService.GetRandomAsync(page);

            if (!items.Success)
            {
                return Result<IReadOnlyList<Media>>.Failure(items.Status.ToString(), ErrorType.Server);
            }

            //Mapear Usando Linq
            var explorerListMapped = _galleryMapper.ToMediaList(items.Data);

            return Result<IReadOnlyList<Media>>.Success(explorerListMapped);
        }

        public async Task<Result<Media>> GetGalleryAlbumById(string id)
        {
            var item = await _apiService.GetGalleryAlbumAsync(id);

            if (!item.Success)
            {
                return Result<Media>.Failure(item.Status.ToString(), ErrorType.Server);
            }

            var albumMediaMapped = this._galleryMapper.ToMedia(item.Data);

            return Result<Media>.Success(albumMediaMapped);
        }
    }
}
