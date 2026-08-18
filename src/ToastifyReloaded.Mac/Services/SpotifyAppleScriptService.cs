using System.Globalization;
using ToastifyReloaded.Mac.Models;

namespace ToastifyReloaded.Mac.Services;

public sealed class SpotifyAppleScriptService
{
    private const char Separator = '\u001f';
    private readonly ProcessService _process;
    private int _lastNonZeroVolume = 50;

    public SpotifyAppleScriptService(ProcessService process)
        => _process = process;

    public Task<bool> IsAvailableAsync()
        => _process.ExistsAsync("osascript");

    public async Task<bool> IsSpotifyAvailableAsync()
    {
        if (!OperatingSystem.IsMacOS() || !await IsAvailableAsync())
            return false;

        var result = await RunScriptAsync(
            "if application \"Spotify\" is running then",
            "return \"yes\"",
            "else",
            "return \"no\"",
            "end if");

        return result.ExitCode == 0 &&
               result.StdOut.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<MacTrackInfo?> GetTrackAsync()
    {
        if (!OperatingSystem.IsMacOS())
            return null;

        var result = await RunScriptAsync(
            "if application \"Spotify\" is not running then return \"\"",
            "tell application \"Spotify\"",
            "try",
            "set tr to current track",
            "set titleText to name of tr as text",
            "set artistText to artist of tr as text",
            "set albumText to album of tr as text",
            "set durationText to duration of tr as text",
            "set positionText to player position as text",
            "set stateText to player state as text",
            "set artText to \"\"",
            "try",
            "set artText to artwork url of tr as text",
            "end try",
            $"return titleText & (character id 31) & artistText & (character id 31) & albumText & (character id 31) & durationText & (character id 31) & positionText & (character id 31) & stateText & (character id 31) & artText",
            "on error",
            "return \"\"",
            "end try",
            "end tell");

        if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.StdOut))
            return null;

        return ParseTrackPayload(result.StdOut);
    }

    internal static MacTrackInfo? ParseTrackPayload(string payload)
    {
        var parts = payload.Trim().Split(Separator);
        if (parts.Length < 7 || string.IsNullOrWhiteSpace(parts[0]))
            return null;

        _ = double.TryParse(
            parts[3],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var durationMs);

        _ = double.TryParse(
            parts[4],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var positionSeconds);

        var state = parts[5].Trim();

        return new MacTrackInfo(
            parts[0].Trim(),
            parts[1].Trim(),
            parts[2].Trim(),
            NormalizeArtwork(parts[6]),
            Math.Max(0, positionSeconds),
            Math.Max(0, durationMs / 1000d),
            state.Equals("playing", StringComparison.OrdinalIgnoreCase));
    }

    public Task PlayPauseAsync()
        => RunSpotifyCommandAsync("playpause");

    public Task NextAsync()
        => RunSpotifyCommandAsync("next track");

    public Task PreviousAsync()
        => RunSpotifyCommandAsync("previous track");

    public Task SeekForwardAsync()
        => RunSpotifyBlockAsync(
            "set p to player position + 10",
            "set d to (duration of current track) / 1000",
            "if p > d then set p to d",
            "set player position to p");

    public Task SeekBackwardAsync()
        => RunSpotifyBlockAsync(
            "set p to player position - 10",
            "if p < 0 then set p to 0",
            "set player position to p");

    public Task VolumeUpAsync()
        => RunSpotifyBlockAsync(
            "set v to sound volume + 5",
            "if v > 100 then set v to 100",
            "set sound volume to v");

    public Task VolumeDownAsync()
        => RunSpotifyBlockAsync(
            "set v to sound volume - 5",
            "if v < 0 then set v to 0",
            "set sound volume to v");

    public async Task ToggleMuteAsync()
    {
        var current = await RunScriptAsync(
            "if application \"Spotify\" is not running then return \"\"",
            "tell application \"Spotify\" to return sound volume as text");

        if (current.ExitCode != 0 ||
            !int.TryParse(current.StdOut, NumberStyles.Integer, CultureInfo.InvariantCulture, out var volume))
            return;

        if (volume > 0)
        {
            _lastNonZeroVolume = volume;
            await RunSpotifyBlockAsync("set sound volume to 0");
        }
        else
        {
            var restore = Math.Clamp(_lastNonZeroVolume, 5, 100);
            await RunSpotifyBlockAsync($"set sound volume to {restore}");
        }
    }

    private async Task RunSpotifyCommandAsync(string command)
    {
        _ = await RunScriptAsync(
            "if application \"Spotify\" is not running then return",
            $"tell application \"Spotify\" to {command}");
    }

    private async Task RunSpotifyBlockAsync(params string[] body)
    {
        var lines = new List<string>
        {
            "if application \"Spotify\" is not running then return",
            "tell application \"Spotify\""
        };

        lines.AddRange(body);
        lines.Add("end tell");
        _ = await RunScriptAsync(lines.ToArray());
    }

    private Task<(int ExitCode, string StdOut, string StdErr)> RunScriptAsync(
        params string[] lines)
    {
        var args = new List<string>(lines.Length * 2);
        foreach (var line in lines)
        {
            args.Add("-e");
            args.Add(line);
        }

        return _process.RunAsync("osascript", args);
    }

    private static string NormalizeArtwork(string value)
    {
        var art = value.Trim();

        if (art.StartsWith("spotify:image:", StringComparison.OrdinalIgnoreCase))
            return "https://i.scdn.co/image/" + art["spotify:image:".Length..];

        if (art.StartsWith("https://open.spotify.com/image/", StringComparison.OrdinalIgnoreCase))
            return art.Replace(
                "https://open.spotify.com/image/",
                "https://i.scdn.co/image/",
                StringComparison.OrdinalIgnoreCase);

        return art;
    }
}
