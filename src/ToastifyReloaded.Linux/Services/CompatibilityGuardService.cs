using ToastifyReloaded.Linux.Models;

namespace ToastifyReloaded.Linux.Services;

public sealed record CompatibilityGuardResult(
    bool Changed,
    bool RepairAttempted,
    bool RepairSucceeded,
    string Message,
    SpotifyInstallInfo? Spotify);

public sealed class CompatibilityGuardService
{
    private readonly SpotifyVersionService _spotifyVersion;
    private readonly SpicetifyLinuxService _spicetify;

    public CompatibilityGuardService(
        SpotifyVersionService spotifyVersion,
        SpicetifyLinuxService spicetify)
    {
        _spotifyVersion = spotifyVersion;
        _spicetify = spicetify;
    }

    public async Task<CompatibilityGuardResult> CheckAsync(
        LinuxSettings settings)
    {
        var spotify = await _spotifyVersion.GetVersionAsync();

        if (spotify is null)
        {
            return new CompatibilityGuardResult(
                false, false, false,
                "GuardNoSpotify",
                null);
        }

        if (string.IsNullOrWhiteSpace(settings.LastSpotifyVersion))
        {
            settings.LastSpotifyVersion = spotify.Version;
            return new CompatibilityGuardResult(
                false, false, false,
                "GuardFirstRun",
                spotify);
        }

        if (string.Equals(
                settings.LastSpotifyVersion,
                spotify.Version,
                StringComparison.OrdinalIgnoreCase))
        {
            return new CompatibilityGuardResult(
                false, false, false,
                "GuardUnchanged",
                spotify);
        }

        if (!settings.EnableCompatibilityGuard ||
            !settings.AutoRepairSpicetify)
        {
            return new CompatibilityGuardResult(
                true, false, false,
                "GuardChanged",
                spotify);
        }

        if (string.Equals(
                settings.LastRepairAttemptVersion,
                spotify.Version,
                StringComparison.OrdinalIgnoreCase))
        {
            return new CompatibilityGuardResult(
                true, true, false,
                "GuardRepairFailed",
                spotify);
        }

        settings.LastRepairAttemptVersion = spotify.Version;

        var repair = await _spicetify.RepairAfterSpotifyUpdateAsync(
            settings.KeepLyricsPlus);

        if (repair.Success)
        {
            settings.LastSpotifyVersion = spotify.Version;
            settings.LastRepairAttemptVersion = "";

            return new CompatibilityGuardResult(
                true, true, true,
                "GuardRepairOk",
                spotify);
        }

        return new CompatibilityGuardResult(
            true, true, false,
            "GuardRepairFailed",
            spotify);
    }
}
