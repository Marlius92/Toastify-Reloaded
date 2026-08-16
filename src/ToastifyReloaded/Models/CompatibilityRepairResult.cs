namespace ToastifyReloaded.Models;

public sealed record CompatibilityRepairResult(
    bool Success,
    string Message,
    string SpicetifyVersion,
    bool UsedFallback);
