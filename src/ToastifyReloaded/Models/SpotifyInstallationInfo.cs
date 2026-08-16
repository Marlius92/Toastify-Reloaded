namespace ToastifyReloaded.Models;

public sealed record SpotifyInstallationInfo(
    string Version,
    string InstallKind,
    string? ExecutablePath)
{
    public static SpotifyInstallationInfo NotFound { get; } = new(string.Empty, "Non rilevato", null);
    public bool IsDetected => !string.IsNullOrWhiteSpace(Version);
}
