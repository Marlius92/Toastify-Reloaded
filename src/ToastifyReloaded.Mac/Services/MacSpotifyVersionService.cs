using System.Text.RegularExpressions;
using ToastifyReloaded.Mac.Models;

namespace ToastifyReloaded.Mac.Services;

public sealed partial class MacSpotifyVersionService
{
    private readonly ProcessService _process;

    public MacSpotifyVersionService(ProcessService process)
        => _process = process;

    public async Task<SpotifyInstallInfo?> GetVersionAsync()
    {
        if (!OperatingSystem.IsMacOS())
            return null;

        foreach (var appPath in CandidateAppPaths())
        {
            var plist = Path.Combine(appPath, "Contents", "Info.plist");
            if (!File.Exists(plist))
                continue;

            try
            {
                var result = await _process.RunAsync(
                    "/usr/libexec/PlistBuddy",
                    new[] { "-c", "Print :CFBundleShortVersionString", plist });

                var version = ExtractVersion(result.StdOut + " " + result.StdErr);
                if (result.ExitCode == 0 && version is not null)
                    return new SpotifyInstallInfo(appPath, version);
            }
            catch
            {
            }
        }

        return null;
    }

    public string? FindSpotifyAppPath()
        => CandidateAppPaths().FirstOrDefault(Directory.Exists);

    private static IEnumerable<string> CandidateAppPaths()
    {
        yield return "/Applications/Spotify.app";
        yield return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Applications",
            "Spotify.app");
    }

    internal static string? ExtractVersion(string value)
    {
        var match = VersionRegex().Match(value);
        return match.Success ? match.Value : null;
    }

    [GeneratedRegex(@"\d+(?:\.\d+){1,4}(?:[-+._a-zA-Z0-9]*)?")]
    private static partial Regex VersionRegex();
}
