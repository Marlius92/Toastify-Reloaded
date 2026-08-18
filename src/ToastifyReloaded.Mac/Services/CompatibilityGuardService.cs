using ToastifyReloaded.Mac.Models;

namespace ToastifyReloaded.Mac.Services;

public sealed record CompatibilityGuardResult(
    bool Changed,
    bool RepairAttempted,
    bool RepairSucceeded,
    string Message,
    SpotifyInstallInfo? Spotify);

public sealed class CompatibilityGuardService
{
    private readonly MacSpotifyVersionService _spotifyVersion;
    private readonly SpicetifyMacService _spicetify;

    public CompatibilityGuardService(
        MacSpotifyVersionService spotifyVersion,
        SpicetifyMacService spicetify)
    {
        _spotifyVersion = spotifyVersion;
        _spicetify = spicetify;
    }

    public async Task<CompatibilityGuardResult> CheckAsync(MacSettings settings)
    {
        var spotify = await _spotifyVersion.GetVersionAsync();

        if (spotify is null)
            return new(false, false, false, "GuardNoSpotify", null);

        if (string.IsNullOrWhiteSpace(settings.LastSpotifyVersion))
        {
            settings.LastSpotifyVersion = spotify.Version;
            return new(false, false, false, "GuardFirstRun", spotify);
        }

        if (string.Equals(settings.LastSpotifyVersion, spotify.Version, StringComparison.OrdinalIgnoreCase))
            return new(false, false, false, "GuardUnchanged", spotify);

        if (!settings.EnableCompatibilityGuard || !settings.AutoRepairSpicetify)
            return new(true, false, false, "GuardChanged", spotify);

        if (string.Equals(settings.LastRepairAttemptVersion, spotify.Version, StringComparison.OrdinalIgnoreCase))
            return new(true, true, false, "GuardRepairFailed", spotify);

        settings.LastRepairAttemptVersion = spotify.Version;
        var repair = await _spicetify.RepairAfterSpotifyUpdateAsync(settings.KeepLyricsPlus);

        if (repair.Success)
        {
            settings.LastSpotifyVersion = spotify.Version;
            settings.LastRepairAttemptVersion = "";
            return new(true, true, true, "GuardRepairOk", spotify);
        }

        return new(true, true, false, "GuardRepairFailed", spotify);
    }
}
