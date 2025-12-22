using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Api.Services.Models.Enum
{
    /// <summary>
    /// Tipos de ordenação da galeria
    /// </summary>
    public enum GallerySort
    {
        /// <summary>
        /// Ordenar por viralidade
        /// </summary>
        Viral,

        /// <summary>
        /// Ordenar pelos mais votados
        /// </summary>
        Top,

        /// <summary>
        /// Ordenar por tempo (mais recentes)
        /// </summary>
        Time,

        /// <summary>
        /// Ordenar por posts em ascensão (apenas para User section)
        /// </summary>
        Rising
    }
}
