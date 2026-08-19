using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToastifyReloaded.Models;

namespace ToastifyReloaded.Services;

public sealed class UpdateService
{
    private const string Owner = "Marlius92";
    private const string Repository = "Toastify-Reloaded";
    private static readonly HttpClient Client = CreateClient();

    public string CurrentVersion => NormalizeVersion(Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0");

    public async Task<UpdateCheckResult> CheckLatestAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Do not use /releases/latest in a multi-platform repository. A Linux or
            // macOS release can legitimately have a numerically newer tag than the
            // Windows line and must never be treated as a Windows update.
            using var response = await Client.GetAsync(
                $"https://api.github.com/repos/{Owner}/{Repository}/releases?per_page=30",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                return Failure($"GitHub ha risposto {(int)response.StatusCode} ({response.ReasonPhrase}).");

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var releases = await JsonSerializer.DeserializeAsync<List<GitHubRelease>>(stream, cancellationToken: cancellationToken);
            if (releases is null || releases.Count == 0)
                return Failure("GitHub non ha restituito release utilizzabili.");

            var assetName = GetExpectedAssetName();
            GitHubRelease? release = null;
            Version? latest = null;
            string? latestText = null;

            foreach (var candidate in releases)
            {
                if (candidate.Draft || candidate.Prerelease || !IsWindowsReleaseTag(candidate.TagName))
                    continue;

                var candidateAsset = candidate.Assets.FirstOrDefault(a =>
                    a.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase));
                if (candidateAsset is null)
                    continue;

                var candidateText = NormalizeVersion(candidate.TagName);
                if (!Version.TryParse(candidateText, out var candidateVersion))
                    continue;

                if (latest is null || candidateVersion > latest)
                {
                    release = candidate;
                    latest = candidateVersion;
                    latestText = candidateText;
                }
            }

            if (release is null || latest is null || latestText is null)
                return Failure($"Nessuna release Windows stabile con l'installer {assetName} è disponibile.");

            if (!Version.TryParse(CurrentVersion, out var current))
                return Failure($"Impossibile confrontare la versione corrente {CurrentVersion}.");

            var asset = release.Assets.First(a =>
                a.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase));

            var available = latest > current;
            var message = available
                ? $"È disponibile Toastify Reloaded {latestText}."
                : $"Toastify Reloaded è aggiornato ({CurrentVersion}).";

            return new UpdateCheckResult(
                true,
                available,
                CurrentVersion,
                latestText,
                release.TagName,
                asset.Name,
                asset.BrowserDownloadUrl,
                release.HtmlUrl,
                message);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Failure("Controllo aggiornamenti scaduto per timeout.");
        }
        catch (Exception ex)
        {
            return Failure($"Controllo aggiornamenti non riuscito: {ex.Message}");
        }
    }

    public async Task<bool> PrepareAndLaunchUpdateAsync(
        UpdateCheckResult update,
        int currentProcessId,
        bool restartMinimized = false,
        CancellationToken cancellationToken = default)
    {
        if (!update.UpdateAvailable || string.IsNullOrWhiteSpace(update.DownloadUrl))
            return false;

        var updateRoot = Path.Combine(
            Path.GetTempPath(),
            "ToastifyReloaded",
            "installer-update-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(updateRoot);

        var installerName = string.IsNullOrWhiteSpace(update.AssetName)
            ? GetExpectedAssetName()
            : Path.GetFileName(update.AssetName);
        var installerPath = Path.Combine(updateRoot, installerName);

        using (var response = await Client.GetAsync(
                   update.DownloadUrl,
                   HttpCompletionOption.ResponseHeadersRead,
                   cancellationToken))
        {
            response.EnsureSuccessStatusCode();
            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var destination = File.Create(installerPath);
            await source.CopyToAsync(destination, cancellationToken);
        }

        ValidatePortableExecutable(installerPath);

        // The installer is signed/packaged separately from the running process.
        // UseShellExecute + runas gives Windows control of the UAC prompt. /S
        // performs the upgrade silently after approval; /UPDATEPID lets NSIS wait
        // for this process to close before replacing the installed executable.
        var installerArguments = $"/S /UPDATEPID={currentProcessId}";
        if (restartMinimized)
            installerArguments += " /RESTARTMINIMIZED=1";

        var startInfo = new ProcessStartInfo
        {
            FileName = installerPath,
            Arguments = installerArguments,
            WorkingDirectory = updateRoot,
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            return Process.Start(startInfo) is not null;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            throw new OperationCanceledException("Aggiornamento annullato nel controllo account utente (UAC).", ex);
        }
    }

    private UpdateCheckResult Failure(string message) =>
        new(false, false, CurrentVersion, string.Empty, null, null, null,
            $"https://github.com/{Owner}/{Repository}/releases/latest", message);

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(25) };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ToastifyReloaded", "1.2"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");
        return client;
    }

    private static bool IsWindowsReleaseTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;

        return !tag.Contains("-linux", StringComparison.OrdinalIgnoreCase)
               && !tag.Contains("-macos", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetExpectedAssetName()
    {
        var runtime = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "win-arm64" : "win-x64";
        return $"ToastifyReloaded-Setup-{runtime}.exe";
    }

    private static void ValidatePortableExecutable(string path)
    {
        using var stream = File.OpenRead(path);
        if (stream.Length < 2 || stream.ReadByte() != 'M' || stream.ReadByte() != 'Z')
            throw new InvalidDataException("L'asset scaricato non è un installer Windows valido.");
    }

    private static string NormalizeVersion(string raw)
    {
        var value = raw.Trim();
        if (value.StartsWith('v') || value.StartsWith('V'))
            value = value[1..];

        var dash = value.IndexOf('-');
        if (dash >= 0)
            value = value[..dash];

        if (Version.TryParse(value, out var parsed))
            return $"{parsed.Major}.{parsed.Minor}.{Math.Max(parsed.Build, 0)}";

        return value;
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;

        [JsonPropertyName("draft")]
        public bool Draft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }

        [JsonPropertyName("assets")]
        public List<GitHubAsset> Assets { get; set; } = [];
    }

    private sealed class GitHubAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("browser_download_url")]
        public string? BrowserDownloadUrl { get; set; }
    }
}
