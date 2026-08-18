namespace ToastifyReloaded.Linux.Services;

public sealed class SpicetifyLinuxService
{
    private readonly ProcessService _process;

    public SpicetifyLinuxService(ProcessService process)
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

        var config = await _process.RunAsync(
            "spicetify",
            new[] { "config", "custom_apps", "lyrics-plus" });

        if (config.ExitCode != 0)
            return (false, config.StdErr);

        var apply = await _process.RunAsync(
            "spicetify",
            new[] { "apply" });

        return apply.ExitCode == 0
            ? (true, "Lyrics Plus applicato.")
            : (false, apply.StdErr);
    }

    public async Task<(bool Success, string Message)> RepairAsync()
        => await RepairAfterSpotifyUpdateAsync(keepLyricsPlus: false);

    public async Task<(bool Success, string Message)> RepairAfterSpotifyUpdateAsync(
        bool keepLyricsPlus)
    {
        if (!await IsAvailableAsync())
            return (false, "Spicetify non trovato.");

        // `upgrade` works only for script-based installations; a failure here is
        // intentionally non-fatal because package-manager installs are valid.
        try
        {
            _ = await _process.RunAsync("spicetify", new[] { "upgrade" });
        }
        catch
        {
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
                return (false, fallback.StdErr);
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
}
