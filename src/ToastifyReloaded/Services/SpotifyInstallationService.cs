using System.Diagnostics;
using System.Text.RegularExpressions;
using ToastifyReloaded.Models;

namespace ToastifyReloaded.Services;

public sealed partial class SpotifyInstallationService
{
    public async Task<SpotifyInstallationInfo> GetInfoAsync()
    {
        var running = TryGetRunningSpotify();
        if (running.IsDetected)
            return running;

        foreach (var path in GetClassicSpotifyPaths())
        {
            if (!File.Exists(path))
                continue;

            var version = TryGetFileVersion(path);
            if (!string.IsNullOrWhiteSpace(version))
                return new SpotifyInstallationInfo(version, "Desktop", path);
        }

        var store = await TryGetStoreSpotifyAsync();
        return store ?? SpotifyInstallationInfo.NotFound;
    }

    private static SpotifyInstallationInfo TryGetRunningSpotify()
    {
        foreach (var process in Process.GetProcessesByName("Spotify"))
        {
            using (process)
            {
                try
                {
                    var path = process.MainModule?.FileName;
                    if (string.IsNullOrWhiteSpace(path))
                        continue;

                    var version = TryGetFileVersion(path);
                    if (!string.IsNullOrWhiteSpace(version))
                        return new SpotifyInstallationInfo(version, "Processo attivo", path);
                }
                catch
                {
                    // Microsoft Store processes can deny MainModule access.
                }
            }
        }

        return SpotifyInstallationInfo.NotFound;
    }

    private static IEnumerable<string> GetClassicSpotifyPaths()
    {
        var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        yield return Path.Combine(roaming, "Spotify", "Spotify.exe");
        yield return Path.Combine(local, "Spotify", "Spotify.exe");
    }

    private static string TryGetFileVersion(string path)
    {
        try
        {
            var info = FileVersionInfo.GetVersionInfo(path);
            return NormalizeVersion(info.FileVersion ?? info.ProductVersion ?? string.Empty);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static async Task<SpotifyInstallationInfo?> TryGetStoreSpotifyAsync()
    {
        const string command = "$p = Get-AppxPackage -Name SpotifyAB.SpotifyMusic -ErrorAction SilentlyContinue | Select-Object -First 1; if ($p) { Write-Output $p.Version.ToString() }";
        try
        {
            var result = await PowerShellService.RunHiddenCommandAsync(command, TimeSpan.FromSeconds(15));
            var version = NormalizeVersion(result.StandardOutput);
            return string.IsNullOrWhiteSpace(version)
                ? null
                : new SpotifyInstallationInfo(version, "Microsoft Store", null);
        }
        catch
        {
            return null;
        }
    }

    private static string NormalizeVersion(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var match = VersionRegex().Match(text);
        return match.Success ? match.Value : text.Trim().Split('\r', '\n')[0].Trim();
    }

    [GeneratedRegex(@"\d+(?:\.\d+){1,4}")]
    private static partial Regex VersionRegex();
}
