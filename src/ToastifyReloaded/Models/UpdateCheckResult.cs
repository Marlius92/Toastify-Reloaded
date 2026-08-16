namespace ToastifyReloaded.Models;

public sealed record UpdateCheckResult(
    bool Success,
    bool UpdateAvailable,
    string CurrentVersion,
    string LatestVersion,
    string? TagName,
    string? AssetName,
    string? DownloadUrl,
    string? ReleasePageUrl,
    string Message);
