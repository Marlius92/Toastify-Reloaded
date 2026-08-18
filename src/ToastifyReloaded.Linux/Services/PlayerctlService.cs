using System.Globalization;
using ToastifyReloaded.Linux.Models;

namespace ToastifyReloaded.Linux.Services;

public sealed class PlayerctlService
{
    private readonly ProcessService _process;
    private double _lastNonZeroVolume = 0.5;
    private string? _cachedPlayer;

    public PlayerctlService(ProcessService process)
        => _process = process;

    public Task<bool> IsAvailableAsync()
        => _process.ExistsAsync("playerctl");

    public async Task<string?> ResolveSpotifyPlayerAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && !string.IsNullOrWhiteSpace(_cachedPlayer))
        {
            var status = await RunRawAsync(
                "--player=" + _cachedPlayer,
                "status");

            if (status.ExitCode == 0)
                return _cachedPlayer;
        }

        var result = await RunRawAsync("--list-all");
        if (result.ExitCode != 0)
        {
            _cachedPlayer = null;
            return null;
        }

        var players = result.StdOut
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        _cachedPlayer =
            players.FirstOrDefault(p => p.Equals("spotify", StringComparison.OrdinalIgnoreCase))
            ?? players.FirstOrDefault(p => p.StartsWith("spotify.instance", StringComparison.OrdinalIgnoreCase))
            ?? players.FirstOrDefault(p => p.StartsWith("spotify", StringComparison.OrdinalIgnoreCase))
            ?? players.FirstOrDefault(p => p.Equals("spotifyd", StringComparison.OrdinalIgnoreCase))
            ?? players.FirstOrDefault(p => p.StartsWith("spotifyd", StringComparison.OrdinalIgnoreCase));

        return _cachedPlayer;
    }

    public async Task<bool> IsSpotifyAvailableAsync()
        => await ResolveSpotifyPlayerAsync(forceRefresh: true) is not null;

    public async Task<LinuxTrackInfo?> GetTrackAsync()
    {
        var player = await ResolveSpotifyPlayerAsync();
        if (player is null)
            return null;

        const string separator = "|||";
        var format =
            "{{title}}" + separator +
            "{{artist}}" + separator +
            "{{album}}" + separator +
            "{{mpris:length}}" + separator +
            "{{mpris:artUrl}}";

        var metadata = await RunPlayerAsync(player, "--format", format, "metadata");

        if (metadata.ExitCode != 0 || string.IsNullOrWhiteSpace(metadata.StdOut))
        {
            // Spotify may have restarted and received a new MPRIS name.
            player = await ResolveSpotifyPlayerAsync(forceRefresh: true);
            if (player is null)
                return null;

            metadata = await RunPlayerAsync(player, "--format", format, "metadata");
        }

        if (metadata.ExitCode != 0 || string.IsNullOrWhiteSpace(metadata.StdOut))
            return null;

        var parts = metadata.StdOut.Split(separator);
        if (parts.Length < 5)
            return null;

        var positionResult = await RunPlayerAsync(player, "position");
        var statusResult = await RunPlayerAsync(player, "status");

        double position = 0;
        _ = double.TryParse(
            positionResult.StdOut,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out position);

        double duration = 0;
        if (double.TryParse(
                parts[3],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var durationMicros))
        {
            duration = durationMicros / 1_000_000d;
        }

        return new LinuxTrackInfo(
            parts[0].Trim(),
            parts[1].Trim(),
            parts[2].Trim(),
            NormalizeArtwork(parts[4]),
            Math.Max(0, position),
            Math.Max(0, duration),
            statusResult.StdOut.Equals("Playing", StringComparison.OrdinalIgnoreCase));
    }

    public Task PlayPauseAsync() => CommandAsync("play-pause");
    public Task NextAsync() => CommandAsync("next");
    public Task PreviousAsync() => CommandAsync("previous");
    public Task SeekForwardAsync() => CommandAsync("position", "10+");
    public Task SeekBackwardAsync() => CommandAsync("position", "10-");
    public Task VolumeUpAsync() => CommandAsync("volume", "0.05+");
    public Task VolumeDownAsync() => CommandAsync("volume", "0.05-");

    public async Task ToggleMuteAsync()
    {
        var player = await ResolveSpotifyPlayerAsync();
        if (player is null)
            return;

        var current = await RunPlayerAsync(player, "volume");
        if (current.ExitCode != 0)
            return;

        if (!double.TryParse(
                current.StdOut,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var volume))
            return;

        if (volume > 0.001)
        {
            _lastNonZeroVolume = volume;
            await RunPlayerAsync(player, "volume", "0");
        }
        else
        {
            await RunPlayerAsync(
                player,
                "volume",
                Math.Clamp(_lastNonZeroVolume, 0.05, 1.0)
                    .ToString("0.00", CultureInfo.InvariantCulture));
        }
    }

    private async Task CommandAsync(params string[] command)
    {
        var player = await ResolveSpotifyPlayerAsync();
        if (player is null)
            return;

        var result = await RunPlayerAsync(player, command);
        if (result.ExitCode != 0)
        {
            _cachedPlayer = null;
            player = await ResolveSpotifyPlayerAsync(forceRefresh: true);
            if (player is not null)
                _ = await RunPlayerAsync(player, command);
        }
    }

    private Task<(int ExitCode, string StdOut, string StdErr)> RunPlayerAsync(
        string player,
        params string[] args)
        => RunRawAsync(new[] { "--player=" + player }.Concat(args).ToArray());

    private Task<(int ExitCode, string StdOut, string StdErr)> RunRawAsync(
        params string[] args)
        => _process.RunAsync("playerctl", args);

    private static string NormalizeArtwork(string value)
    {
        var art = value.Trim();

        if (art.StartsWith("https://open.spotify.com/image/", StringComparison.OrdinalIgnoreCase))
            return art.Replace(
                "https://open.spotify.com/image/",
                "https://i.scdn.co/image/",
                StringComparison.OrdinalIgnoreCase);

        return art;
    }
}
