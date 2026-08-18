namespace ToastifyReloaded.Linux.Models;

public sealed record LinuxUpdateAsset(
    string Name,
    Uri DownloadUri,
    long Size,
    string? Digest);

public sealed record LinuxUpdateInfo(
    LinuxReleaseVersion Version,
    Uri ReleaseUri,
    IReadOnlyList<LinuxUpdateAsset> Assets)
{
    public string Tag => Version.Tag;
}
