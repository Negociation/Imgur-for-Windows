using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Api.Services.Models.Enum
{

    /// <summary>
    /// Janela de tempo para filtros
    /// </summary>
    public enum GalleryWindow
    {
        /// <summary>
        /// Últimas 24 horas
        /// </summary>
        Day,

        /// <summary>
        /// Última semana
        /// </summary>
        Week,

        /// <summary>
        /// Último mês
        /// </summary>
        Month,

        /// <summary>
        /// Último ano
        /// </summary>
        Year,

        /// <summary>
        /// Todo o histórico
        /// </summary>
        All
    }
}
