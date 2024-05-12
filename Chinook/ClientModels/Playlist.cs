namespace Chinook.ClientModels;

public class Playlist
{
    public long PlayListId { get; set; }
    public string Name { get; set; }
    public List<PlaylistTrack> Tracks { get; set; }
}