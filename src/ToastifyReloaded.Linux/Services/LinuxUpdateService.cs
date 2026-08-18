using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using ToastifyReloaded.Linux.Models;

namespace ToastifyReloaded.Linux.Services;

public sealed partial class LinuxUpdateService
{
    public const string CurrentTag = "v1.4.0-linux-rc.1";

    public static readonly LinuxReleaseVersion CurrentVersion =
        ParseTag(CurrentTag)
        ?? throw new InvalidOperationException(
            "Invalid current Linux release tag.");

    private static readonly Uri ReleasesApi = new(
        "https://api.github.com/repos/Marlius92/Toastify-Reloaded/releases?per_page=50");

    private readonly HttpClient _http;

    public LinuxUpdateService()
    {
        _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        _http.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(
                "ToastifyReloaded",
                "1.4.0-rc.1"));

        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                "application/vnd.github+json"));

        _http.DefaultRequestHeaders.Add(
            "X-GitHub-Api-Version",
            "2022-11-28");
    }

    public async Task<LinuxUpdateInfo?> CheckAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync(
            ReleasesApi,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        using var document =
            await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);

        LinuxUpdateInfo? best = null;

        foreach (var release in document.RootElement.EnumerateArray())
        {
            if (release.TryGetProperty(
                    "draft",
                    out var draft) &&
                draft.GetBoolean())
            {
                continue;
            }

            if (!release.TryGetProperty(
                    "tag_name",
                    out var tagNode))
            {
                continue;
            }

            var tag = tagNode.GetString() ?? "";
            var version = ParseTag(tag);

            if (version is null ||
                version.CompareTo(CurrentVersion) <= 0)
            {
                continue;
            }

            var url = release.TryGetProperty(
                "html_url",
                out var urlNode)
                ? urlNode.GetString()
                : null;

            if (!Uri.TryCreate(
                    url,
                    UriKind.Absolute,
                    out var releaseUri))
            {
                continue;
            }

            var assets = ParseAssets(release);

            var candidate = new LinuxUpdateInfo(
                version,
                releaseUri,
                assets);

            if (best is null ||
                candidate.Version.CompareTo(best.Version) > 0)
            {
                best = candidate;
            }
        }

        return best;
    }

    internal static LinuxReleaseVersion? ParseTag(
        string tag)
    {
        var match = LinuxTagRegex().Match(tag);

        if (!match.Success)
            return null;

        if (!int.TryParse(match.Groups["major"].Value, out var major) ||
            !int.TryParse(match.Groups["minor"].Value, out var minor) ||
            !int.TryParse(match.Groups["patch"].Value, out var patch))
        {
            return null;
        }

        var channel =
            match.Groups["channel"].Value
                .ToLowerInvariant();

        var stage =
            channel switch
            {
                "preview" => LinuxReleaseStage.Preview,
                "rc" => LinuxReleaseStage.ReleaseCandidate,
                "" => LinuxReleaseStage.Stable,
                _ => throw new InvalidOperationException()
            };

        var stageNumber = 0;

        if (stage != LinuxReleaseStage.Stable &&
            !int.TryParse(
                match.Groups["number"].Value,
                out stageNumber))
        {
            return null;
        }

        return new LinuxReleaseVersion(
            major,
            minor,
            patch,
            stage,
            stageNumber,
            tag);
    }

    private static IReadOnlyList<LinuxUpdateAsset> ParseAssets(
        JsonElement release)
    {
        var assets = new List<LinuxUpdateAsset>();

        if (!release.TryGetProperty(
                "assets",
                out var assetsNode) ||
            assetsNode.ValueKind != JsonValueKind.Array)
        {
            return assets;
        }

        foreach (var asset in assetsNode.EnumerateArray())
        {
            var name = asset.TryGetProperty(
                "name",
                out var nameNode)
                ? nameNode.GetString()
                : null;

            var download = asset.TryGetProperty(
                "browser_download_url",
                out var downloadNode)
                ? downloadNode.GetString()
                : null;

            var size = asset.TryGetProperty(
                "size",
                out var sizeNode)
                ? sizeNode.GetInt64()
                : 0;

            var digest = asset.TryGetProperty(
                "digest",
                out var digestNode)
                ? digestNode.GetString()
                : null;

            if (string.IsNullOrWhiteSpace(name) ||
                !Uri.TryCreate(
                    download,
                    UriKind.Absolute,
                    out var downloadUri))
            {
                continue;
            }

            assets.Add(
                new LinuxUpdateAsset(
                    name,
                    downloadUri,
                    size,
                    digest));
        }

        return assets;
    }

    [GeneratedRegex(
        @"^v(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)-linux(?:-(?<channel>preview|rc)\.(?<number>\d+))?$",
        RegexOptions.IgnoreCase)]
    private static partial Regex LinuxTagRegex();
}
