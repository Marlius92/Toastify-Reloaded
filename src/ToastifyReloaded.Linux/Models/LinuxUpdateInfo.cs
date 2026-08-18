namespace ToastifyReloaded.Linux.Models;

public sealed record LinuxUpdateInfo(
    string Tag,
    int PreviewNumber,
    Uri ReleaseUri);
