using Chinook.ClientModels;
using Chinook.Models;
using Chinook.Services.Interfaces;
using Chinook.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Chinook.Pages
{
    public partial class ArtistPage : ComponentBase
    {
        [Inject] private IPlaylistService _playlistService { get; set; }
        [Inject] private IUnitOfWork _unitOfWork { get; set; }
        [Inject] private AppState _appState { get; set; }

        [Parameter] public long ArtistId { get; set; }

        [CascadingParameter] private Task<AuthenticationState> authenticationState { get; set; }
        [Inject] IDbContextFactory<ChinookContext> DbFactory { get; set; }

        private Modal PlaylistDialog { get; set; }

        private Artist Artist;
        private List<PlaylistTrack> Tracks;
        private DbContext DbContext;
        private PlaylistTrack SelectedTrack;
        private string InfoMessage;
        private string CurrentUserId;
        private List<ClientModels.Playlist> UserPlaylists = new List<ClientModels.Playlist>();
        private long SelectedPlaylistId { get; set; }
        private string NewPlaylistName { get; set; }
        private string ErrorMessage;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
            CurrentUserId = await GetUserId();
            var DbContext = await DbFactory.CreateDbContextAsync();

            Artist = DbContext.Artists.SingleOrDefault(a => a.ArtistId == ArtistId);

            Tracks = DbContext.Tracks.Where(a => a.Album.ArtistId == ArtistId)
                .Include(a => a.Album)
                .Select(t => new PlaylistTrack()
                {
                    AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == CurrentUserId && up.Playlist.Name == "My favorite tracks")).Any()
                })
                .ToList();
        }

        private async Task<string> GetUserId()
        {
            var user = (await authenticationState).User;
            var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
            return userId;
        }

        private async void FavoriteTrack(long trackId)
        {
            var isNewPlayListAdded = false;
            var track = Tracks.FirstOrDefault(t => t.TrackId == trackId);

            var playlist = await _playlistService.GetPlaylistByNameAsync("My favorite tracks");
            if (playlist is null)
            {
                // Create the playlist if it doesn't exist
                playlist = await _playlistService.CreateNewPlayListAsync("My favorite tracks");
                isNewPlayListAdded = true;
            }

            if (!isNewPlayListAdded)
            {
                var userPlaylist = _unitOfWork.GetDatabaseContext().UserPlaylists
                                           .FirstOrDefault(up =>
                                               up.UserId == CurrentUserId &&
                                               up.PlaylistId == playlist.PlaylistId
                                           );
                if (userPlaylist is null)
                {
                    isNewPlayListAdded = true;
                }
            }

            await _playlistService.CreateNewUserPlayListAsync(CurrentUserId, playlist.PlaylistId);

            // Add the track to the playlist using the association table
            await _playlistService.CreateNewPlaylistTrackAsync(playlist.PlaylistId, trackId);

            await _unitOfWork.GetDatabaseContext().SaveChangesAsync();

            Tracks = await _playlistService.GetPlaylistTracksByArtistAsync(ArtistId, CurrentUserId);

            if (isNewPlayListAdded)
            {
                _appState.UpdatePlaylistStore();
            }

            InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} added to playlist Favorites.";
        }

        private async void UnfavoriteTrack(long trackId)
        {
            var track = Tracks.FirstOrDefault(t => t.TrackId == trackId);

            var playlist = await _playlistService.GetPlaylistByNameAsync("My favorite tracks");
            if (playlist is not null)
            {
                await _playlistService.DeletePlaylistTrackAsync(playlist.PlaylistId, trackId);
                await _unitOfWork.GetDatabaseContext().SaveChangesAsync();
            }

            Tracks = await _playlistService.GetPlaylistTracksByArtistAsync(ArtistId, CurrentUserId);

            InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} removed from playlist Favorites.";
        }

        private async void OpenPlaylistDialog(long trackId)
        {
            CloseInfoMessage();
            ResetErrorMessage();
            SelectedTrack = Tracks.FirstOrDefault(t => t.TrackId == trackId);
            UserPlaylists = await _playlistService.GetPlayListByUserIdAsync(CurrentUserId);
            SelectedPlaylistId = 0;
            NewPlaylistName = "";
            PlaylistDialog.Open();
        }

        private async void AddTrackToPlaylist()
        {
            var isNewPlayListAdded = false;
            ResetErrorMessage();

            // Get the selected playlist based on the dropdown value
            var playlistId = SelectedPlaylistId;

            Chinook.Models.Playlist playlist = null;

            if (playlistId == 0)
            {
                // If no playlist is selected, create a new one
                var newPlaylistName = NewPlaylistName?.Trim();

                if (string.IsNullOrWhiteSpace(newPlaylistName))
                {
                    ErrorMessage = "Please enter a valid name for the new playlist.";
                    return;
                }

                var existingPlayList = await _playlistService.GetPlaylistByNameAsync(newPlaylistName);
                if (existingPlayList is not null)
                {
                    ErrorMessage = "Playlist name already exists. Please enter a different name.";
                    return;
                }

                playlist = await _playlistService.CreateNewPlayListAsync(newPlaylistName);
                await _playlistService.CreateNewUserPlayListAsync(CurrentUserId, playlist.PlaylistId);
                isNewPlayListAdded = true;
            }
            else
            {
                // If an existing playlist is selected, fetch it from the database
                playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);

                if (playlist == null)
                {
                    ErrorMessage = "Selected playlist not found.";
                    return;
                }
            }

            var existingPlaylistTrack = await _unitOfWork.GetDatabaseContext().Set<Dictionary<string, object>>("PlaylistTrack")
                        .FirstOrDefaultAsync(pt => pt["PlaylistId"].Equals(playlist.PlaylistId) && pt["TrackId"].Equals(SelectedTrack.TrackId));

            if (existingPlaylistTrack is not null)
            {
                ErrorMessage = $"The track '{SelectedTrack.TrackName}' has already been added to the playlist '{playlist.Name}'.";
                return;
            }

            // Add the track to the playlist using the association table
            await _playlistService.CreateNewPlaylistTrackAsync(playlist.PlaylistId, SelectedTrack.TrackId);

            await _unitOfWork.GetDatabaseContext().SaveChangesAsync();

            // Clear the new playlist name/dropdown after adding the track
            NewPlaylistName = "";
            SelectedPlaylistId = 0;

            CloseInfoMessage();

            InfoMessage = $"Track {Artist.Name} - {SelectedTrack.AlbumTitle} - {SelectedTrack.TrackName} added to playlist {playlist.Name}.";

            PlaylistDialog.Close();

            UserPlaylists = await _playlistService.GetPlayListByUserIdAsync(CurrentUserId);
            Tracks = await _playlistService.GetPlaylistTracksByArtistAsync(ArtistId, CurrentUserId);

            if (isNewPlayListAdded)
            {
                _appState.UpdatePlaylistStore();
            }
        }

        private void CloseInfoMessage()
        {
            InfoMessage = "";
        }

        private void HandlePlaylistSelection(ChangeEventArgs e)
        {
            SelectedPlaylistId = e.Value != "" ? Convert.ToInt64(e.Value) : 0;
            NewPlaylistName = "";
        }

        private void ResetErrorMessage()
        {
            ErrorMessage = "";
        }
    }
}
