using Chinook.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Chinook.Pages
{
    public partial class PlaylistPage : ComponentBase
    {
        [Inject] private IPlaylistService _playlistService { get; set; }
        [Inject] private IUnitOfWork _unitOfWork { get; set; }

        [Parameter] public long PlaylistId { get; set; }
        [Inject] IDbContextFactory<ChinookContext> DbFactory { get; set; }
        [CascadingParameter] private Task<AuthenticationState> authenticationState { get; set; }

        private Chinook.ClientModels.Playlist Playlist;
        private string CurrentUserId;
        private string InfoMessage;

        protected override async Task OnInitializedAsync()
        {
            CurrentUserId = await GetUserId();

            await InvokeAsync(StateHasChanged);
            var DbContext = await DbFactory.CreateDbContextAsync();

            Playlist = await DbContext.Playlists
                            .Include(x => x.UserPlaylists).ThenInclude(x => x.UserPlaylistTracks)
                            .ThenInclude(a => a.Track).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
                            .Where(p => p.PlaylistId == PlaylistId && p.UserPlaylists.Any(x=>x.UserId == CurrentUserId))
                            .Select(p => new ClientModels.Playlist()
                            {
                                Name = p.Name,
                                Tracks = p.UserPlaylists.Where(up => up.UserId == CurrentUserId).SelectMany(x => x.UserPlaylistTracks).Select(x=>x.Track)
                                .Select(t => new ClientModels.PlaylistTrack()
                                {
                                    AlbumTitle = t.Album.Title,
                                    ArtistName = t.Album.Artist.Name,
                                    TrackId = t.TrackId,
                                    TrackName = t.Name,
                                    IsFavorite = t.UserPlaylistTracks.Any(x => x.TrackId == t.TrackId && x.UserId == CurrentUserId && x.UserPlaylist.Playlist.Name.Equals("My favorite tracks"))
                                }).ToList()
                            })
                            .FirstOrDefaultAsync();
        }

        private async Task<string> GetUserId()
        {
            var user = (await authenticationState).User;
            var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
            return userId;
        }

        private async void FavoriteTrack(long trackId)
        {
            var track = Playlist.Tracks.FirstOrDefault(t => t.TrackId == trackId);

            var playlist = await _playlistService.GetPlaylistByNameAsync("My favorite tracks");
            if (playlist is null)
            {
                // Create the playlist if it doesn't exist
                playlist = await _playlistService.CreateNewPlayListAsync("My favorite tracks");
            }

            await _playlistService.CreateNewUserPlayListAsync(CurrentUserId, playlist.PlaylistId);

            // Add the track to the playlist using the association table
            await _playlistService.CreateNewUserPlaylistTrackAsync(playlist.PlaylistId, trackId, CurrentUserId);

            await _unitOfWork.GetDatabaseContext().SaveChangesAsync();

            Playlist = await _playlistService.GetPlaylistTracksByPlaylistIdAsync(PlaylistId, CurrentUserId);

            InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} added to playlist Favorites.";
        }

        private async void UnfavoriteTrack(long trackId)
        {
            var track = Playlist.Tracks.FirstOrDefault(t => t.TrackId == trackId);

            var playlist = await _playlistService.GetPlaylistByNameAsync("My favorite tracks");
            if (playlist is not null)
            {
                await _playlistService.DeleteUserPlaylistTrackAsync(playlist.PlaylistId, trackId, CurrentUserId);
                await _unitOfWork.GetDatabaseContext().SaveChangesAsync();
            }

            Playlist = await _playlistService.GetPlaylistTracksByPlaylistIdAsync(PlaylistId, CurrentUserId);

            InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} removed from playlist Favorites.";
        }

        private async void RemoveTrack(long trackId)
        {
            await _playlistService.DeleteUserPlaylistTrackAsync(PlaylistId, trackId, CurrentUserId);
            await _unitOfWork.GetDatabaseContext().SaveChangesAsync();

            Playlist = await _playlistService.GetPlaylistTracksByPlaylistIdAsync(PlaylistId, CurrentUserId);
            CloseInfoMessage();
        }

        private void CloseInfoMessage()
        {
            InfoMessage = "";
        }

        protected override async Task OnParametersSetAsync()
        {
            await OnInitializedAsync();
        }
    }
}
