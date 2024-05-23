using Chinook.ClientModels;

namespace Chinook.Services.Interfaces
{
    public interface IPlaylistService
    {
        Task<Chinook.Models.Playlist> GetPlaylistByNameAsync(string playlistName);
        Task<Chinook.Models.Playlist> GetPlaylistByIdAsync(long playlistId);
        Task<List<PlaylistTrack>> GetPlaylistTracksByArtistAsync(long artistId, string userId);
        Task<List<ClientModels.Playlist>> GetPlayListByUserIdAsync(string userId);
        Task<Chinook.Models.Playlist> CreateNewPlayListAsync(string newPlaylistName);
        Task CreateNewUserPlayListAsync(string currentUserId, long playlistId);
        Task CreateNewPlaylistTrackAsync(long playlistId, long trackId);
        Task CreateNewUserPlaylistTrackAsync(long playlistId, long trackId, string userId);
        Task DeletePlaylistTrackAsync(long playlistId, long trackId);
        Task DeleteUserPlaylistTrackAsync(long playlistId, long trackId, string userId);
        Task<ClientModels.Playlist> GetPlaylistTracksByPlaylistIdAsync(long playlistId, string userId);
    }
}
