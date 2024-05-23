namespace Chinook.Models
{
    public class UserPlaylistTrack
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public long PlaylistId { get; set; }
        public long TrackId { get; set; }

        public virtual UserPlaylist UserPlaylist { get; set; }
        public virtual Track Track { get; set; }
    }
}
