namespace ToastifyReloaded.Mac.Models;

public sealed record MacUpdateAsset(
    string Name,
    Uri DownloadUri,
    long Size,
    string? Digest);

public sealed record MacUpdateInfo(
    MacReleaseVersion Version,
    Uri ReleaseUri,
    IReadOnlyList<MacUpdateAsset> Assets)
{
    public string Tag => Version.Tag;
}
