using Imgur.Enums;
using Imgur.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Imgur.Services
{
    public class UrlHandlerService
    {
        private const string ImgurDomain = "imgur.com";
        private const string ImgurSecureDomain = "https://imgur.com";


        private readonly GalleryService _galleryService;
        private readonly AlbumService _albumService;
        private readonly TagsService _tagService;
        private readonly AccountService _accountService;

        public UrlHandlerService(
            GalleryService galleryService,
            AlbumService albumService,
            TagsService tagService,
            AccountService accountService
            )
        {
            _galleryService = galleryService;
            _albumService = albumService;
            _tagService = tagService;
            _accountService = accountService;
        }

        /// <summary>
        /// Processa uma URL do Imgur e identifica seu tipo, retornando os dados da API
        /// </summary>
        public async Task<ImgurUrl> HandleAsync(string url)
        {
            var result = new ImgurUrl
            {
                OriginalUrl = url,
                IsValid = false
            };

            try
            {
                if (!IsImgurUrl(url))
                {
                    result.ErrorMessage = "URL não é do domínio Imgur";
                    return result;
                }

                var uri = new Uri(url);
                var segments = uri.Segments
                    .Select(s => s.Trim('/'))
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();

                if (segments.Length == 0)
                {
                    result.ErrorMessage = "URL inválida";
                    return result;
                }

                // Switch para diferentes tipos de handler
                switch (segments[0].ToLower())
                {
                    case "gallery":
                        return await GalleryHandlerAsync(segments, result);
                    case "a":
                        //Check for Gallery First
                        Debug.WriteLine("Verificar Galeria");
                        var galleryResult = await GalleryHandlerAsync(segments, result, ImgurUrlType.Album);
                        if (galleryResult.IsValid)
                        {
                            return galleryResult;
                        }
                        Debug.WriteLine("Verificar Album");
                        //Check Default Album ( Less Info )
                        return await AlbumHandlerAsync(segments, result);
                    case "t":
                        return await TagHandlerAsync(segments, result);
                    case "user":
                        return await UserHandlerAsync(segments, result);

                }

                result.ErrorMessage = "Tipo de URL não suportado";
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Erro ao processar URL: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Verifica se a URL é do domínio Imgur
        /// </summary>
        private bool IsImgurUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            try
            {
                var uri = new Uri(url);
                return uri.Host.EndsWith(ImgurDomain, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Handler para URLs do tipo /gallery/
        /// </summary>
        private async Task<ImgurUrl> GalleryHandlerAsync(string[] segments, ImgurUrl result, ImgurUrlType imgurUrlType = ImgurUrlType.Gallery)
        {
            if (segments.Length < 2)
            {
                result.ErrorMessage = "URL de gallery inválida";
                return result;
            }

            string sanitizedId = SanitizeId(segments[1]);
            result.Id = sanitizedId;

            var apiResult = await _galleryService.GetGalleryAlbumById(sanitizedId);

            if (!apiResult.IsSuccess)
            {
                result.ErrorMessage = "Não foi possível recuperar na API";
                return result;
            }

            result.IsValid = true;
            result.Type = imgurUrlType;
            result.Data = apiResult.Data;

            return result;
        }

        /// <summary>
        /// Handler para URLs do tipo /a/
        /// </summary>
        private async Task<ImgurUrl> AlbumHandlerAsync(string[] segments, ImgurUrl result)
        {
            if (segments.Length < 2)
            {
                result.ErrorMessage = "URL de album inválida";
                return result;
            }

            string sanitizedId = SanitizeId(segments[1]);
            result.Id = sanitizedId;

            var apiResult = await _albumService.GetAlbumById(sanitizedId);

            if (!apiResult.IsSuccess)
            {
                result.ErrorMessage = "Não foi possível recuperar na API";
                return result;
            }

            result.IsValid = true;
            result.Type = ImgurUrlType.Album;
            result.Data = apiResult.Data;

            return result;
        }

        /// <summary>
        /// Handler para URLs do tipo /a/
        /// </summary>
        private async Task<ImgurUrl> TagHandlerAsync(string[] segments, ImgurUrl result)
        {
            if (segments.Length < 2)
            {
                result.ErrorMessage = "URL de album inválida";
                return result;
            }

            string sanitizedId = SanitizeId(segments[1]);
            result.Id = sanitizedId;

            var apiResult = await _tagService.GetTagById(sanitizedId);

            if (!apiResult.IsSuccess)
            {
                result.ErrorMessage = "Não foi possível recuperar na API";
                return result;
            }

            result.IsValid = true;
            result.Type = ImgurUrlType.Tag;
            result.Data = apiResult.Data;

            return result;
        }

        /// <summary>
        /// Handler para URLs do tipo /user/
        /// </summary>
        private async Task<ImgurUrl> UserHandlerAsync(string[] segments, ImgurUrl result)
        {
            if (segments.Length < 2)
            {
                result.ErrorMessage = "URL de album inválida";
                return result;
            }

            string sanitizedId = SanitizeId(segments[1]);
            result.Id = sanitizedId;

            var apiResult = await _accountService.GetAccountById(sanitizedId);

            if (!apiResult.IsSuccess)
            {
                result.ErrorMessage = "Não foi possível recuperar na API";
                return result;
            }

            result.IsValid = true;
            result.Type = ImgurUrlType.Account;
            result.Data = apiResult.Data;

            return result;
        }
        /// <summary>
        /// Sanitiza o ID removendo slugs longos e pegando apenas o identificador real
        /// Ex: "ice-terrorizing-neighborhoods-pepper-spray-children-97CRKak" -> "97CRKak"
        /// </summary>
        private string SanitizeId(string rawId)
        {
            if (string.IsNullOrWhiteSpace(rawId))
                return string.Empty;

            // Remove extensão se houver (.jpg, .png, etc)
            rawId = System.IO.Path.GetFileNameWithoutExtension(rawId);

            // Se contém hífen, provavelmente é um slug
            // O ID real geralmente está no final após o último hífen
            if (rawId.Contains("-"))
            {
                var parts = rawId.Split('-');
                // Pega a última parte que normalmente é o ID
                return parts[parts.Length - 1];
            }

            return rawId;
        }
    }
}