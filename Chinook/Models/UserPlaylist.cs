namespace Chinook.Models;

public class UserPlaylist
{
    public UserPlaylist()
    {
        UserPlaylistTracks = new HashSet<UserPlaylistTrack>();
    }

    public string UserId { get; set; }
    public long PlaylistId { get; set; }
    public ChinookUser User { get; set; }
    public virtual Playlist Playlist { get; set; }

    public virtual ICollection<UserPlaylistTrack> UserPlaylistTracks { get; set; }
}
