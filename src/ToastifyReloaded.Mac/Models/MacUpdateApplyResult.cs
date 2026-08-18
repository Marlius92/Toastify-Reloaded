namespace ToastifyReloaded.Mac.Models;

public sealed record MacUpdateApplyResult(
    bool Success,
    bool RestartStarted,
    string Message,
    string? DownloadedPath);
