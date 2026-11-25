using Imgur.Models;

namespace Imgur.Helpers
{
    public static class AlbumPartialLoading
    {
        /// <summary>
        /// Álbum parcial = a contagem declarada de imagens difere dos Elements já carregados.
        /// Elements pode ser nulo (ex.: Media mínimo criado a partir de um comentário);
        /// nesse caso a comparação usa null e não lança exceção.
        /// </summary>
        public static bool IsPartial(Media media)
        {
            if (media == null || !media.IsAlbum)
                return false;

            return media.ImagesCount != media.Elements?.Count;
        }
    }
}
