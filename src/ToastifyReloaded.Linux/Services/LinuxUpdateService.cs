using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using ToastifyReloaded.Linux.Models;

namespace ToastifyReloaded.Linux.Services;

public sealed partial class LinuxUpdateService
{
    public const int CurrentPreviewNumber = 4;
    public const string CurrentTag = "v1.4.0-linux-preview.4";

    private static readonly Uri ReleasesApi = new(
        "https://api.github.com/repos/Marlius92/Toastify-Reloaded/releases?per_page=30");

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
                "1.4.0-preview.4"));

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
            if (release.TryGetProperty("draft", out var draft) &&
                draft.GetBoolean())
                continue;

            if (!release.TryGetProperty("tag_name", out var tagNode))
                continue;

            var tag = tagNode.GetString() ?? "";
            var match = PreviewTagRegex().Match(tag);

            if (!match.Success)
                continue;

            if (!int.TryParse(
                    match.Groups[1].Value,
                    out var preview))
                continue;

            if (preview <= CurrentPreviewNumber)
                continue;

            var url = release.TryGetProperty(
                "html_url",
                out var urlNode)
                ? urlNode.GetString()
                : null;

            if (!Uri.TryCreate(
                    url,
                    UriKind.Absolute,
                    out var releaseUri))
                continue;

            var assets = new List<LinuxUpdateAsset>();

            if (release.TryGetProperty(
                    "assets",
                    out var assetsNode) &&
                assetsNode.ValueKind == JsonValueKind.Array)
            {
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
                        continue;

                    assets.Add(
                        new LinuxUpdateAsset(
                            name,
                            downloadUri,
                            size,
                            digest));
                }
            }

            var candidate = new LinuxUpdateInfo(
                tag,
                preview,
                releaseUri,
                assets);

            if (best is null ||
                preview > best.PreviewNumber)
            {
                best = candidate;
            }
        }

        return best;
    }

    [GeneratedRegex(
        @"^v1\.4\.0-linux-preview\.(\d+)$",
        RegexOptions.IgnoreCase)]
    private static partial Regex PreviewTagRegex();
}
