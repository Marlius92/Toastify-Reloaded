using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using ToastifyReloaded.Mac.Models;

namespace ToastifyReloaded.Mac.Services;

public sealed class MacUpdateInstallerService
{
    private readonly ProcessService _process;
    private readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    public MacUpdateInstallerService(ProcessService process)
    {
        _process = process;
        _http.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("ToastifyReloaded", "1.5.0"));
    }

    public async Task<MacUpdateApplyResult> DownloadAndApplyAsync(
        MacUpdateInfo update,
        CancellationToken cancellationToken = default)
    {
        var asset = SelectAsset(update, RuntimeInformation.ProcessArchitecture);
        if (asset is null)
        {
            return new MacUpdateApplyResult(
                false,
                false,
                "Nessun pacchetto macOS compatibile trovato nella release.",
                null);
        }

        var downloaded = await DownloadAsync(update, asset, cancellationToken);
        if (!await VerifyDigestAsync(downloaded, asset.Digest, cancellationToken))
        {
            File.Delete(downloaded);
            return new MacUpdateApplyResult(
                false,
                false,
                "Verifica SHA-256 del pacchetto non riuscita.",
                null);
        }

        var staging = Path.Combine(
            Path.GetDirectoryName(downloaded)!,
            "staging-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);

        var extract = await _process.RunAsync(
            "/usr/bin/ditto",
            new[] { "-x", "-k", downloaded, staging },
            cancellationToken);

        if (extract.ExitCode != 0)
        {
            return new MacUpdateApplyResult(
                false,
                false,
                string.IsNullOrWhiteSpace(extract.StdErr)
                    ? "Estrazione aggiornamento macOS non riuscita."
                    : extract.StdErr,
                downloaded);
        }

        var newApp = Directory
            .EnumerateDirectories(staging, "*.app", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        if (newApp is null)
        {
            return new MacUpdateApplyResult(
                false,
                false,
                "Il pacchetto aggiornamento non contiene un bundle .app.",
                downloaded);
        }

        var currentApp = FindCurrentAppBundle();
        if (currentApp is null)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "/usr/bin/open",
                UseShellExecute = false,
                ArgumentList = { newApp }
            });

            return new MacUpdateApplyResult(
                true,
                true,
                "Aggiornamento estratto e nuova app avviata.",
                newApp);
        }

        var helper = CreateReplacementHelper(currentApp, newApp, Environment.ProcessId);
        Process.Start(new ProcessStartInfo
        {
            FileName = "/bin/sh",
            UseShellExecute = false,
            CreateNoWindow = true,
            ArgumentList = { helper }
        });

        return new MacUpdateApplyResult(
            true,
            true,
            "Aggiornamento macOS scaricato. Toastify Reloaded verrà sostituito e riavviato; macOS può richiedere l'autorizzazione amministratore.",
            downloaded);
    }

    internal static MacUpdateAsset? SelectAsset(
        MacUpdateInfo update,
        Architecture architecture)
    {
        var expected = architecture == Architecture.Arm64
            ? "ToastifyReloaded-macOS-arm64.zip"
            : "ToastifyReloaded-macOS-x64.zip";

        return update.Assets.FirstOrDefault(x =>
            string.Equals(x.Name, expected, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<string> DownloadAsync(
        MacUpdateInfo update,
        MacUpdateAsset asset,
        CancellationToken cancellationToken)
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library",
            "Caches",
            "ToastifyReloaded",
            "updates",
            update.Tag);

        Directory.CreateDirectory(directory);
        var destination = Path.Combine(directory, asset.Name);

        using var response = await _http.GetAsync(
            asset.DownloadUri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var output = File.Create(destination);
        await input.CopyToAsync(output, cancellationToken);
        return destination;
    }

    private static async Task<bool> VerifyDigestAsync(
        string path,
        string? digest,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(digest) ||
            !digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
            return true;

        var expected = digest["sha256:".Length..].Trim().ToLowerInvariant();
        await using var stream = File.OpenRead(path);
        var bytes = await SHA256.HashDataAsync(stream, cancellationToken);
        var actual = Convert.ToHexString(bytes).ToLowerInvariant();
        return string.Equals(expected, actual, StringComparison.Ordinal);
    }

    internal static string? FindCurrentAppBundle()
    {
        var processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath))
            return null;

        var directory = new DirectoryInfo(Path.GetDirectoryName(processPath)!);
        while (directory is not null)
        {
            if (directory.Name.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
                return directory.FullName;
            directory = directory.Parent;
        }

        return null;
    }

    private static string CreateReplacementHelper(string currentApp, string newApp, int pid)
    {
        var helperRoot = Path.Combine(
            Path.GetTempPath(),
            "ToastifyReloaded-update-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(helperRoot);

        var helper = Path.Combine(helperRoot, "apply-update.sh");
        var backup = currentApp + ".previous";

        var replacementCommand = string.Join("; ", new[]
        {
            $"/bin/rm -rf {ShellQuote(backup)}",
            $"/bin/mv {ShellQuote(currentApp)} {ShellQuote(backup)}",
            $"if /usr/bin/ditto {ShellQuote(newApp)} {ShellQuote(currentApp)}; then /bin/rm -rf {ShellQuote(backup)}; /usr/bin/open {ShellQuote(currentApp)}; else /bin/rm -rf {ShellQuote(currentApp)}; /bin/mv {ShellQuote(backup)} {ShellQuote(currentApp)}; /usr/bin/open {ShellQuote(currentApp)}; exit 1; fi"
        });

        var appleScriptCommand = replacementCommand
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);

        var script = $"""#!/bin/sh
set -u
while /bin/kill -0 {pid} 2>/dev/null; do
  /bin/sleep 0.25
done
/usr/bin/osascript -e 'do shell script "{appleScriptCommand}" with administrator privileges'
/bin/rm -rf {ShellQuote(helperRoot)}
""";

        File.WriteAllText(helper, script);
        File.SetUnixFileMode(
            helper,
            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        return helper;
    }

    private static string ShellQuote(string value)
        => "'" + value.Replace("'", "'\\''", StringComparison.Ordinal) + "'";
}
