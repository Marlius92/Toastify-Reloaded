namespace ToastifyReloaded.Linux.Models;

public sealed record LinuxUpdateApplyResult(
    bool Success,
    bool RestartStarted,
    string Message,
    string? DownloadedPath);
