namespace Chinook.ClientModels
{
    public class ArtistDto
    {
        public long ArtistId { get; set; }
        public string? Name { get; set; }
        public int AlbumCount { get; set; }
        public AlbumDto AlbumList { get; set; }
    }
}
