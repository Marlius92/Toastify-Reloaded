namespace ToastifyReloaded.Linux.Models;

public sealed record LinuxTrackInfo(
    string Title,
    string Artist,
    string Album,
    string ArtworkUrl,
    double PositionSeconds,
    double DurationSeconds,
    bool IsPlaying)
{
    public string Identity => $"{Title}\u001f{Artist}\u001f{Album}";
}
