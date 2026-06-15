namespace Imgur.Models
{
    public class FileUploadResult
    {
        public string ImageId { get; set; }
        public string AlbumId { get; set; }
        public bool IsAlbum => AlbumId != null;
    }
}