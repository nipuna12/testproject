using Chinook.ClientModels;
using Chinook.Models;
using Chinook.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services.Implementations
{
    public class PlayListService : IPlaylistService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PlayListService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Chinook.Models.Playlist> GetPlaylistByNameAsync(string playlistName)
        {
            var dbContext = _unitOfWork.GetDatabaseContext();
            return await dbContext.Playlists.FirstOrDefaultAsync(p => p.Name == playlistName);
        }

        public async Task<Chinook.Models.Playlist> GetPlaylistByIdAsync(long playlistId)
        {
            var dbContext = _unitOfWork.GetDatabaseContext();
            return await dbContext.Playlists.FindAsync(playlistId);
        }

        public async Task<List<PlaylistTrack>> GetPlaylistTracksByArtistAsync(long artistId, string userId)
        {
            var dbContext = _unitOfWork.GetDatabaseContext();
            var tracks = await dbContext.Tracks.Where(a => a.Album.ArtistId == artistId)
                        .Include(a => a.Album)
                        .Select(t => new PlaylistTrack()
                        {
                            AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                            TrackId = t.TrackId,
                            TrackName = t.Name,
                            IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.Name == "My favorite tracks")).Any()
                        })
                        .ToListAsync();

            return tracks;
        }

        public async Task<List<ClientModels.Playlist>> GetPlayListByUserIdAsync(string userId)
        {
            if(userId is null)
            {
                return new List<ClientModels.Playlist>();
            }

            var dbContext = _unitOfWork.GetDatabaseContext();

            var playlist = await GetPlaylistByNameAsync("My favorite tracks");
            if (playlist is not null)
            {
                // Create the user playlist if it doesn't exist
                await CreateNewUserPlayListAsync(userId, playlist.PlaylistId);
                await dbContext.SaveChangesAsync();
            }

            var userPlaylists = await dbContext.Playlists
                .Include(x => x.UserPlaylists)
                .Where(x => x.UserPlaylists.Any(x => x.UserId == userId))
                .Select(up => new ClientModels.Playlist()
                {
                    PlayListId = up.PlaylistId,
                    Name = up.Name
                })
                .ToListAsync();

            return userPlaylists;
        }

        public async Task<Chinook.Models.Playlist> CreateNewPlayListAsync(string newPlaylistName)
        {
            var dbContext = _unitOfWork.GetDatabaseContext();
            var playlist = new Chinook.Models.Playlist
            {
                PlaylistId = dbContext.Playlists.Any() ? dbContext.Playlists.AsNoTracking().Max(x => x.PlaylistId) + 1 : 1,
                Name = newPlaylistName
            };
            dbContext.Playlists.Add(playlist);

            return playlist;
        }

        public async Task CreateNewUserPlayListAsync(string currentUserId, long playlistId)
        {
            var dbContext = _unitOfWork.GetDatabaseContext();

            var userPlaylist = dbContext.UserPlaylists
                                       .FirstOrDefault(up =>
                                           up.UserId == currentUserId &&
                                           up.PlaylistId == playlistId
                                       );
            if (userPlaylist is null)
            {
                // Create the user playlist if it doesn't exist
                userPlaylist = new UserPlaylist
                {
                    UserId = currentUserId,
                    PlaylistId = playlistId,
                };

                dbContext.UserPlaylists.Add(userPlaylist);
            }
        }

        public async Task CreateNewPlaylistTrackAsync(long playlistId, long trackId)
        {
            var dbContext = _unitOfWork.GetDatabaseContext();

            var playlistTrack = new Dictionary<string, object>
        {
            { "PlaylistId", playlistId },
            { "TrackId", trackId }
        };

            var existingPlaylistTrack = await dbContext.Set<Dictionary<string, object>>("PlaylistTrack")
                    .FirstOrDefaultAsync(pt => pt["PlaylistId"].Equals(playlistId) && pt["TrackId"].Equals(trackId));

            if (existingPlaylistTrack is null)
            {
                dbContext.Set<Dictionary<string, object>>("PlaylistTrack").Add(playlistTrack);
            }
        }

        public async Task DeletePlaylistTrackAsync(long playlistId, long trackId)
        {
            var dbContext = _unitOfWork.GetDatabaseContext();
            var playlistTrack = await dbContext.Set<Dictionary<string, object>>("PlaylistTrack")
                    .FirstOrDefaultAsync(pt => pt["PlaylistId"].Equals(playlistId) && pt["TrackId"].Equals(trackId));

            if (playlistTrack is not null)
            {
                dbContext.Set<Dictionary<string, object>>("PlaylistTrack").Remove(playlistTrack);
            }
        }

        public async Task<ClientModels.Playlist> GetPlaylistTracksByPlaylistIdAsync(long playlistId, string userId)
        {
            var dbContext = _unitOfWork.GetDatabaseContext();
            var playlists = await dbContext.Playlists
                            .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
                            .Where(p => p.PlaylistId == playlistId)
                            .Select(p => new ClientModels.Playlist()
                            {
                                Name = p.Name,
                                Tracks = p.Tracks.Select(t => new ClientModels.PlaylistTrack()
                                {
                                    AlbumTitle = t.Album.Title,
                                    ArtistName = t.Album.Artist.Name,
                                    TrackId = t.TrackId,
                                    TrackName = t.Name,
                                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.Name == "My favorite tracks")).Any()
                                }).ToList()
                            })
                            .FirstOrDefaultAsync();

            return playlists;
        }
    }
}
