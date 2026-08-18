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
    {
        if (!await IsAvailableAsync())
            return (false, "Spicetify non trovato.");

        var primary = await _process.RunAsync(
            "spicetify",
            new[] { "backup", "apply" });

        if (primary.ExitCode == 0)
            return (true, "Riparazione Spicetify completata.");

        var fallback = await _process.RunAsync(
            "spicetify",
            new[] { "restore", "backup", "apply" });

        return fallback.ExitCode == 0
            ? (true, "Riparazione completa Spicetify eseguita.")
            : (false, fallback.StdErr);
    }
}
