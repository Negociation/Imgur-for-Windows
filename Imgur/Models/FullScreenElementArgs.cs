using System;

namespace Imgur.Models
{
    /// <summary>
    /// Pedido partilhado (Imgur) para abrir elemento em fullscreen no RootFrame.
    /// </summary>
    public sealed class FullScreenElementArgs
    {
        public Element Element { get; set; }
        public Media Post { get; set; }
        public string AuthorUsername { get; set; }
        public Action FavoriteChanged { get; set; }
    }
}
