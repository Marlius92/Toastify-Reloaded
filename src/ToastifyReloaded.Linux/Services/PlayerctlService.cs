using System.Globalization;
using ToastifyReloaded.Linux.Models;

namespace ToastifyReloaded.Linux.Services;

public sealed class PlayerctlService
{
    private readonly ProcessService _process;
    private double _lastNonZeroVolume = 0.5;

    public PlayerctlService(ProcessService process)
        => _process = process;

    public Task<bool> IsAvailableAsync()
        => _process.ExistsAsync("playerctl");

    public async Task<bool> IsSpotifyAvailableAsync()
    {
        var result = await RunAsync("--list-all");
        if (result.ExitCode != 0)
            return false;

        return result.StdOut
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Any(x => x.Trim().StartsWith("spotify", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<LinuxTrackInfo?> GetTrackAsync()
    {
        const string separator = "|||";
        var format =
            "{{title}}" + separator +
            "{{artist}}" + separator +
            "{{album}}" + separator +
            "{{mpris:length}}" + separator +
            "{{mpris:artUrl}}";

        var metadata = await RunAsync("--player=spotify", "--format", format, "metadata");
        if (metadata.ExitCode != 0 || string.IsNullOrWhiteSpace(metadata.StdOut))
            return null;

        var parts = metadata.StdOut.Split(separator);
        if (parts.Length < 5)
            return null;

        var positionResult = await RunAsync("--player=spotify", "position");
        var statusResult = await RunAsync("--player=spotify", "status");

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
            parts[4].Trim(),
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
        var current = await RunAsync("--player=spotify", "volume");
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
            await CommandAsync("volume", "0");
        }
        else
        {
            await CommandAsync(
                "volume",
                Math.Clamp(_lastNonZeroVolume, 0.05, 1.0)
                    .ToString("0.00", CultureInfo.InvariantCulture));
        }
    }

    private async Task CommandAsync(params string[] command)
        => _ = await RunAsync(new[] { "--player=spotify" }.Concat(command).ToArray());

    private Task<(int ExitCode, string StdOut, string StdErr)> RunAsync(params string[] args)
        => _process.RunAsync("playerctl", args);
}
