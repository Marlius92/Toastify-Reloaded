using System.Text.RegularExpressions;
using ToastifyReloaded.Linux.Models;

namespace ToastifyReloaded.Linux.Services;

public sealed partial class SpotifyVersionService
{
    private readonly ProcessService _process;

    public SpotifyVersionService(ProcessService process)
        => _process = process;

    public async Task<SpotifyInstallInfo?> GetVersionAsync()
    {
        if (await _process.ExistsAsync("spotify"))
        {
            try
            {
                var result = await _process.RunAsync("spotify", new[] { "--version" });
                var version = ExtractVersion(result.StdOut + " " + result.StdErr);
                if (result.ExitCode == 0 && version is not null)
                    return new SpotifyInstallInfo("native", version);
            }
            catch
            {
                // Continue with package-manager detection.
            }
        }

        if (await _process.ExistsAsync("flatpak"))
        {
            try
            {
                var result = await _process.RunAsync(
                    "flatpak",
                    new[] { "info", "--show-version", "com.spotify.Client" });

                if (result.ExitCode == 0 && !string.IsNullOrWhiteSpace(result.StdOut))
                    return new SpotifyInstallInfo("flatpak", result.StdOut.Trim());
            }
            catch
            {
            }
        }

        if (await _process.ExistsAsync("dpkg-query"))
        {
            try
            {
                var result = await _process.RunAsync(
                    "dpkg-query",
                    new[] { "-W", "-f=${Version}", "spotify-client" });

                if (result.ExitCode == 0 && !string.IsNullOrWhiteSpace(result.StdOut))
                    return new SpotifyInstallInfo("deb", result.StdOut.Trim());
            }
            catch
            {
            }
        }

        if (await _process.ExistsAsync("snap"))
        {
            try
            {
                var result = await _process.RunAsync("snap", new[] { "list", "spotify" });
                if (result.ExitCode == 0)
                {
                    var lines = result.StdOut.Split(
                        '\n',
                        StringSplitOptions.RemoveEmptyEntries |
                        StringSplitOptions.TrimEntries);

                    if (lines.Length >= 2)
                    {
                        var columns = Regex.Split(lines[1], @"\s+");
                        if (columns.Length >= 2)
                            return new SpotifyInstallInfo("snap", columns[1]);
                    }
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private static string? ExtractVersion(string value)
    {
        var match = VersionRegex().Match(value);
        return match.Success ? match.Value : null;
    }

    [GeneratedRegex(@"\d+(?:\.\d+){1,4}(?:[-+._a-zA-Z0-9]*)?")]
    private static partial Regex VersionRegex();
}
