namespace ToastifyReloaded.Linux.Models;

public sealed record LinuxUpdateAsset(
    string Name,
    Uri DownloadUri,
    long Size,
    string? Digest);

public sealed record LinuxUpdateInfo(
    string Tag,
    int PreviewNumber,
    Uri ReleaseUri,
    IReadOnlyList<LinuxUpdateAsset> Assets);
