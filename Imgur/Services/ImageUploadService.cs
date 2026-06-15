
using global::Imgur.Api.Services.Contracts;
using global::Imgur.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgur.Services
{
    public class ImageUploadService
    {
        private readonly IImageService _imageService;
        private readonly IAlbumService _albumApiService;

        public ImageUploadService(
            IImageService imageService,
            IAlbumService albumApiService)
        {
            _imageService = imageService;
            _albumApiService = albumApiService;
        }

        public async Task<Result<FileUploadResult>> UploadAsync(
            IList<SelectedFile> files,
            string title = null,
            string description = null,
            bool isPrivate = false)
        {
            if (files == null || files.Count == 0)
                return Result<FileUploadResult>.Failure("Nenhum arquivo selecionado.");

            try
            {
                var uploadResult = new FileUploadResult();
                string albumId = null;

                if (files.Count > 1)
                {
                    var albumResponse = await _albumApiService.CreateAlbumAsync(title, description, isPrivate);
                    if (!albumResponse.Success)
                        return Result<FileUploadResult>.Failure("Erro ao criar album.");

                    albumId = albumResponse.Data.Id;
                    uploadResult.AlbumId = albumId;
                }

                foreach (var file in files)
                {
                    if (file.Bytes == null || file.Bytes.Length == 0)
                        return Result<FileUploadResult>.Failure($"Arquivo {file.Name} inválido.");

                    var result = await _imageService.UploadAsync(
                        file.Bytes,
                        file.Name,
                        file.MimeType,
                        files.Count == 1 ? title : null,
                        files.Count == 1 ? description : null,
                        albumId);

                    if (!result.Success)
                        return Result<FileUploadResult>.Failure($"Erro ao enviar {file.Name}.");

                    if (files.Count == 1)
                        uploadResult.ImageId = result.Data.Id;
                }

                return Result<FileUploadResult>.Success(uploadResult);
            }
            catch (Exception ex)
            {
                return Result<FileUploadResult>.Failure($"Erro inesperado: {ex.Message}.");
            }
        }
    }
}

