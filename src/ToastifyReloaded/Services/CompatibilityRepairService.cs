using System.Diagnostics;
using ToastifyReloaded.Models;

namespace ToastifyReloaded.Services;

public sealed class CompatibilityRepairService
{
    public async Task<string> GetSpicetifyVersionAsync()
    {
        const string command = "$c = Get-Command spicetify -ErrorAction SilentlyContinue; if (-not $c) { exit 127 }; spicetify --version";
        try
        {
            var result = await PowerShellService.RunHiddenCommandAsync(command, TimeSpan.FromSeconds(20));
            if (!result.Success)
                return string.Empty;

            return FirstUsefulLine(result.StandardOutput);
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<CompatibilityRepairResult> RepairAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        var spicetifyVersion = await GetSpicetifyVersionAsync();
        if (string.IsNullOrWhiteSpace(spicetifyVersion))
        {
            return new CompatibilityRepairResult(
                false,
                "Spicetify non è installato o non è disponibile nel PATH. Usa prima 'Installa / abilita Lyrics Plus'.",
                string.Empty,
                false);
        }

        await StopSpotifyAsync();

        if (settings.AutoUpgradeSpicetify)
        {
            // 'upgrade' is the official CLI self-update command for script-based installations.
            // Package-managed installations may reject it; that must not abort recovery.
            await RunSpicetifyAsync("spicetify upgrade", cancellationToken, allowFailure: true);
        }

        var primary = await RunSpicetifyAsync("spicetify backup apply", cancellationToken, allowFailure: true);
        var usedFallback = false;
        if (!primary.Success)
        {
            usedFallback = true;
            var fallback = await RunSpicetifyAsync("spicetify restore backup apply", cancellationToken, allowFailure: true);
            if (!fallback.Success)
            {
                var error = CleanError(fallback.StandardError, fallback.StandardOutput);
                return new CompatibilityRepairResult(
                    false,
                    $"Spicetify non è riuscito a riapplicarsi alla nuova versione di Spotify. {error}".Trim(),
                    spicetifyVersion,
                    true);
            }
        }

        if (settings.KeepLyricsPlusEnabled)
        {
            var configuredApps = await RunSpicetifyAsync("spicetify config custom_apps", cancellationToken, allowFailure: true);
            if (!configuredApps.StandardOutput.Contains("lyrics-plus", StringComparison.OrdinalIgnoreCase))
            {
                var addLyrics = await RunSpicetifyAsync("spicetify config custom_apps lyrics-plus", cancellationToken, allowFailure: true);
                if (!addLyrics.Success)
                {
                    return new CompatibilityRepairResult(
                        false,
                        "Spotify è stato riparato, ma non sono riuscito a riabilitare Lyrics Plus.",
                        spicetifyVersion,
                        usedFallback);
                }
            }

            var applyLyrics = await RunSpicetifyAsync("spicetify apply", cancellationToken, allowFailure: true);
            if (!applyLyrics.Success)
            {
                return new CompatibilityRepairResult(
                    false,
                    "Spotify è stato riparato, ma l'applicazione finale di Lyrics Plus non è riuscita.",
                    spicetifyVersion,
                    usedFallback);
            }
        }

        if (settings.RestartSpotifyAfterRepair)
        {
            // Spicetify documents 'auto' as: backup if needed, apply, then launch Spotify.
            var launch = await RunSpicetifyAsync("spicetify auto", cancellationToken, allowFailure: true, timeout: TimeSpan.FromMinutes(2));
            if (!launch.Success)
            {
                try
                {
                    Process.Start(new ProcessStartInfo("spotify:") { UseShellExecute = true });
                }
                catch
                {
                    // Repair is still considered successful; the user can open Spotify manually.
                }
            }
        }

        return new CompatibilityRepairResult(
            true,
            usedFallback
                ? "Riparazione completata con ricostruzione completa del backup Spicetify."
                : "Riparazione automatica completata correttamente.",
            spicetifyVersion,
            usedFallback);
    }

    private static async Task StopSpotifyAsync()
    {
        var processes = Process.GetProcessesByName("Spotify");
        foreach (var process in processes)
        {
            using (process)
            {
                try
                {
                    if (process.CloseMainWindow())
                        await Task.Run(() => process.WaitForExit(1800));

                    if (!process.HasExited)
                        process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Continue with other Spotify processes. Spicetify will report if files stay locked.
                }
            }
        }

        if (processes.Length > 0)
            await Task.Delay(1200);
    }

    private static async Task<PowerShellCommandResult> RunSpicetifyAsync(
        string command,
        CancellationToken cancellationToken,
        bool allowFailure,
        TimeSpan? timeout = null)
    {
        var result = await PowerShellService.RunHiddenCommandAsync(
            "$ErrorActionPreference='Continue'; " + command + "; exit $LASTEXITCODE",
            timeout ?? TimeSpan.FromMinutes(4),
            cancellationToken);

        if (!allowFailure && !result.Success)
            throw new InvalidOperationException(CleanError(result.StandardError, result.StandardOutput));

        return result;
    }

    private static string FirstUsefulLine(string text) =>
        text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line)) ?? string.Empty;

    private static string CleanError(string error, string output)
    {
        var text = string.IsNullOrWhiteSpace(error) ? output : error;
        return FirstUsefulLine(text);
    }
}
