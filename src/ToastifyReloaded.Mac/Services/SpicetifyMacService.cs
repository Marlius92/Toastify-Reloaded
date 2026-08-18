namespace ToastifyReloaded.Mac.Services;

public sealed class SpicetifyMacService
{
    private readonly ProcessService _process;

    public SpicetifyMacService(ProcessService process)
        => _process = process;

    public Task<bool> IsAvailableAsync()
        => _process.ExistsAsync("spicetify");

    public async Task<string> GetVersionAsync()
    {
        try
        {
            var result = await _process.RunAsync("spicetify", new[] { "-v" });
            return result.ExitCode == 0 ? result.StdOut : "not available";
        }
        catch
        {
            return "not available";
        }
    }

    public async Task<(bool Success, string Message)> EnableLyricsAsync()
    {
        if (!await IsAvailableAsync())
            return (false, "Spicetify non trovato.");

        await EnsureSpotifyPathAsync();

        var config = await _process.RunAsync(
            "spicetify",
            new[] { "config", "custom_apps", "lyrics-plus" });

        if (config.ExitCode != 0)
            return (false, PreferError(config));

        var apply = await _process.RunAsync("spicetify", new[] { "apply" });
        return apply.ExitCode == 0
            ? (true, "Lyrics Plus applicato.")
            : (false, PreferError(apply));
    }

    public Task<(bool Success, string Message)> RepairAsync()
        => RepairAfterSpotifyUpdateAsync(keepLyricsPlus: false);

    public async Task<(bool Success, string Message)> RepairAfterSpotifyUpdateAsync(
        bool keepLyricsPlus)
    {
        if (!await IsAvailableAsync())
            return (false, "Spicetify non trovato.");

        await EnsureSpotifyPathAsync();

        try
        {
            _ = await _process.RunAsync("spicetify", new[] { "update" });
        }
        catch
        {
            // Homebrew installations may not support `spicetify update`.
        }

        var primary = await _process.RunAsync(
            "spicetify",
            new[] { "backup", "apply" });

        if (primary.ExitCode != 0)
        {
            var fallback = await _process.RunAsync(
                "spicetify",
                new[] { "restore", "backup", "apply" });

            if (fallback.ExitCode != 0)
                return (false, PreferError(fallback));
        }

        if (keepLyricsPlus)
        {
            var lyrics = await _process.RunAsync(
                "spicetify",
                new[] { "config", "custom_apps", "lyrics-plus" });

            if (lyrics.ExitCode == 0)
                _ = await _process.RunAsync("spicetify", new[] { "apply" });
        }

        return (true, "Riparazione Spicetify completata.");
    }

    private async Task EnsureSpotifyPathAsync()
    {
        var path = "/Applications/Spotify.app/Contents/Resources";
        if (!Directory.Exists(path))
        {
            path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Applications",
                "Spotify.app",
                "Contents",
                "Resources");
        }

        if (!Directory.Exists(path))
            return;

        try
        {
            _ = await _process.RunAsync(
                "spicetify",
                new[] { "config", "spotify_path", path });
        }
        catch
        {
        }
    }

    private static string PreferError((int ExitCode, string StdOut, string StdErr) result)
        => !string.IsNullOrWhiteSpace(result.StdErr) ? result.StdErr : result.StdOut;
}
