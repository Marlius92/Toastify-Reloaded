using ToastifyReloaded.Models;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace ToastifyReloaded.Services;

public sealed class SpotifySessionService
{
    private const ulong MaxArtworkBytes = 8UL * 1024UL * 1024UL;

    private GlobalSystemMediaTransportControlsSessionManager? _manager;
    private string? _cachedArtworkIdentity;
    private byte[]? _cachedArtwork;

    public async Task InitializeAsync()
    {
        _manager ??= await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
    }

    public async Task<TrackInfo?> GetCurrentTrackAsync()
    {
        var session = await GetSpotifySessionAsync();
        if (session is null)
            return null;

        try
        {
            var properties = await session.TryGetMediaPropertiesAsync();
            var title = string.IsNullOrWhiteSpace(properties.Title) ? "Titolo sconosciuto" : properties.Title;
            var artist = string.IsNullOrWhiteSpace(properties.Artist) ? "Artista sconosciuto" : properties.Artist;
            var album = properties.AlbumTitle ?? string.Empty;
            var identity = $"{title}\u001f{artist}\u001f{album}";

            byte[]? artwork;
            if (identity.Equals(_cachedArtworkIdentity, StringComparison.Ordinal) && _cachedArtwork is not null)
            {
                artwork = _cachedArtwork;
            }
            else
            {
                artwork = await TryReadArtworkAsync(properties.Thumbnail);
                if (artwork is not null)
                {
                    _cachedArtworkIdentity = identity;
                    _cachedArtwork = artwork;
                }
                else if (!identity.Equals(_cachedArtworkIdentity, StringComparison.Ordinal))
                {
                    _cachedArtworkIdentity = null;
                    _cachedArtwork = null;
                }
            }

            var timeline = session.GetTimelineProperties();
            var start = timeline.StartTime;
            var rawPosition = timeline.Position - start;
            var rawDuration = timeline.EndTime - start;
            var position = rawPosition < TimeSpan.Zero ? TimeSpan.Zero : rawPosition;
            var duration = rawDuration > TimeSpan.Zero ? rawDuration : TimeSpan.Zero;
            var playback = session.GetPlaybackInfo();
            var isPlaying = playback?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

            return new TrackInfo(title, artist, album, artwork, position, duration, isPlaying);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> PlayPauseAsync()
    {
        var session = await GetSpotifySessionAsync();
        return session is not null && await session.TryTogglePlayPauseAsync();
    }

    public async Task<bool> NextAsync()
    {
        var session = await GetSpotifySessionAsync();
        return session is not null && await session.TrySkipNextAsync();
    }

    public async Task<bool> PreviousAsync()
    {
        var session = await GetSpotifySessionAsync();
        return session is not null && await session.TrySkipPreviousAsync();
    }

    public async Task<bool> SeekRelativeAsync(TimeSpan delta)
    {
        var session = await GetSpotifySessionAsync();
        if (session is null)
            return false;

        var timeline = session.GetTimelineProperties();
        var target = timeline.Position + delta;
        if (target < TimeSpan.Zero)
            target = TimeSpan.Zero;
        if (timeline.EndTime > TimeSpan.Zero && target > timeline.EndTime)
            target = timeline.EndTime;

        return await session.TryChangePlaybackPositionAsync(target.Ticks);
    }

    public async Task<string> GetConnectionDescriptionAsync()
    {
        var session = await GetSpotifySessionAsync();
        return session is null
            ? "Spotify non rilevato"
            : $"Connesso: {session.SourceAppUserModelId}";
    }

    private static async Task<byte[]?> TryReadArtworkAsync(IRandomAccessStreamReference? thumbnail)
    {
        if (thumbnail is null)
            return null;

        try
        {
            using var stream = await thumbnail.OpenReadAsync();
            if (stream.Size == 0 || stream.Size > MaxArtworkBytes || stream.Size > uint.MaxValue)
                return null;

            var requested = (uint)stream.Size;
            using var reader = new DataReader(stream.GetInputStreamAt(0));
            var loaded = await reader.LoadAsync(requested);
            if (loaded == 0)
                return null;

            var bytes = new byte[(int)loaded];
            reader.ReadBytes(bytes);
            return bytes;
        }
        catch
        {
            // Artwork is optional. A missing/broken thumbnail must never break media controls.
            return null;
        }
    }

    private async Task<GlobalSystemMediaTransportControlsSession?> GetSpotifySessionAsync()
    {
        await InitializeAsync();
        if (_manager is null)
            return null;

        return _manager.GetSessions()
            .FirstOrDefault(s => s.SourceAppUserModelId.Contains("spotify", StringComparison.OrdinalIgnoreCase));
    }
}
