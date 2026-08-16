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
            using var response = await Client.GetAsync(
                $"https://api.github.com/repos/{Owner}/{Repository}/releases/latest",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                return Failure($"GitHub ha risposto {(int)response.StatusCode} ({response.ReasonPhrase}).");

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var release = await JsonSerializer.DeserializeAsync<GitHubRelease>(stream, cancellationToken: cancellationToken);
            if (release is null || string.IsNullOrWhiteSpace(release.TagName))
                return Failure("La Latest Release di GitHub non contiene un tag valido.");

            var latestText = NormalizeVersion(release.TagName);
            if (!Version.TryParse(CurrentVersion, out var current) || !Version.TryParse(latestText, out var latest))
                return Failure($"Impossibile confrontare le versioni {CurrentVersion} e {latestText}.");

            var assetName = GetExpectedAssetName();
            var asset = release.Assets.FirstOrDefault(a =>
                a.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase));

            var available = latest > current;
            var message = available
                ? asset is null
                    ? $"È disponibile Toastify Reloaded {latestText}, ma manca l'installer {assetName}."
                    : $"È disponibile Toastify Reloaded {latestText}."
                : $"Toastify Reloaded è aggiornato ({CurrentVersion}).";

            return new UpdateCheckResult(
                true,
                available,
                CurrentVersion,
                latestText,
                release.TagName,
                asset?.Name,
                asset?.BrowserDownloadUrl,
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
        var startInfo = new ProcessStartInfo
        {
            FileName = installerPath,
            Arguments = $"/S /UPDATEPID={currentProcessId}",
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
