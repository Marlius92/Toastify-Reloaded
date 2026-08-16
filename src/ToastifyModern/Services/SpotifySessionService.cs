using ToastifyModern.Models;
using Windows.Media.Control;

namespace ToastifyModern.Services;

public sealed class SpotifySessionService
{
    private GlobalSystemMediaTransportControlsSessionManager? _manager;

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
            return new TrackInfo(
                string.IsNullOrWhiteSpace(properties.Title) ? "Titolo sconosciuto" : properties.Title,
                string.IsNullOrWhiteSpace(properties.Artist) ? "Artista sconosciuto" : properties.Artist,
                properties.AlbumTitle ?? string.Empty);
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

    private async Task<GlobalSystemMediaTransportControlsSession?> GetSpotifySessionAsync()
    {
        await InitializeAsync();
        if (_manager is null)
            return null;

        return _manager.GetSessions()
            .FirstOrDefault(s => s.SourceAppUserModelId.Contains("spotify", StringComparison.OrdinalIgnoreCase));
    }
}
