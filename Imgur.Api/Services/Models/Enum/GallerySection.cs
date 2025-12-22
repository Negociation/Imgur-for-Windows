using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Api.Services.Models.Enum
{
    /// <summary>
    /// Seções da galeria do Imgur
    /// </summary>
    public enum GallerySection
    {
        /// <summary>
        /// Posts mais populares no momento
        /// </summary>
        Hot,

        /// <summary>
        /// Posts mais votados
        /// </summary>
        Top,

        /// <summary>
        /// Posts submetidos pelos usuários
        /// </summary>
        User
    }
}
