namespace Imgur.Models
{
    /// <summary>
    /// Primeira visualização de uma gallery/album/image (persistência local).
    /// </summary>
    public class ViewedMedia
    {
        /// <summary>Chave estável: accountKey|mediaId</summary>
        public string Id { get; set; }

        public string MediaId { get; set; }

        /// <summary>Id/username da conta, ou string vazia se anónimo.</summary>
        public string AccountKey { get; set; }

        /// <summary>Unix timestamp (segundos) da primeira vista.</summary>
        public long ViewedAt { get; set; }
    }
}
