using System.Diagnostics;
using System.IO.Compression;
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
            {
                return Failure($"GitHub ha risposto {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

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
                    ? $"È disponibile Toastify Reloaded {latestText}, ma manca l'asset {assetName}."
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

        var targetDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var updateRoot = Path.Combine(Path.GetTempPath(), "ToastifyReloaded", "update-" + Guid.NewGuid().ToString("N"));
        var zipPath = Path.Combine(updateRoot, "package.zip");
        var payloadDirectory = Path.Combine(updateRoot, "payload");
        Directory.CreateDirectory(payloadDirectory);

        using (var response = await Client.GetAsync(update.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
        {
            response.EnsureSuccessStatusCode();
            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var destination = File.Create(zipPath);
            await source.CopyToAsync(destination, cancellationToken);
        }

        ZipFile.ExtractToDirectory(zipPath, payloadDirectory, overwriteFiles: true);
        var newExecutable = Path.Combine(payloadDirectory, "ToastifyReloaded.exe");
        if (!File.Exists(newExecutable))
            throw new InvalidDataException("Il pacchetto della Release non contiene ToastifyReloaded.exe.");

        var updaterScript = Path.Combine(updateRoot, "install-update.ps1");
        File.WriteAllText(updaterScript, BuildUpdaterScript());

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = updateRoot
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(updaterScript);
        startInfo.ArgumentList.Add("-ProcessId");
        startInfo.ArgumentList.Add(currentProcessId.ToString());
        startInfo.ArgumentList.Add("-Source");
        startInfo.ArgumentList.Add(payloadDirectory);
        startInfo.ArgumentList.Add("-Target");
        startInfo.ArgumentList.Add(targetDirectory);
        startInfo.ArgumentList.Add("-Root");
        startInfo.ArgumentList.Add(updateRoot);

        return Process.Start(startInfo) is not null;
    }

    private UpdateCheckResult Failure(string message) =>
        new(false, false, CurrentVersion, string.Empty, null, null, null,
            $"https://github.com/{Owner}/{Repository}/releases/latest", message);

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(25) };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ToastifyReloaded", "1.1"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");
        return client;
    }

    private static string GetExpectedAssetName()
    {
        var runtime = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "win-arm64" : "win-x64";
        return $"ToastifyReloaded-{runtime}.zip";
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

    private static string BuildUpdaterScript() => """
param(
    [Parameter(Mandatory=$true)][int]$ProcessId,
    [Parameter(Mandatory=$true)][string]$Source,
    [Parameter(Mandatory=$true)][string]$Target,
    [Parameter(Mandatory=$true)][string]$Root
)
$ErrorActionPreference = 'Stop'
$log = Join-Path $env:TEMP 'ToastifyReloaded-update.log'
try {
    "[$(Get-Date -Format o)] Update start -> $Target" | Set-Content -Path $log
    try { Wait-Process -Id $ProcessId -Timeout 90 -ErrorAction Stop } catch { Start-Sleep -Seconds 2 }
    Start-Sleep -Milliseconds 750

    Get-ChildItem -LiteralPath $Source -Force | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $Target -Recurse -Force
    }

    $exe = Join-Path $Target 'ToastifyReloaded.exe'
    if (-not (Test-Path -LiteralPath $exe)) { throw 'ToastifyReloaded.exe non trovato dopo la copia.' }
    "[$(Get-Date -Format o)] Update copied successfully" | Add-Content -Path $log
    Start-Process -FilePath $exe
    Start-Sleep -Seconds 2
    Remove-Item -LiteralPath $Root -Recurse -Force -ErrorAction SilentlyContinue
} catch {
    "[$(Get-Date -Format o)] ERROR: $($_.Exception.Message)" | Add-Content -Path $log
}
""";

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
