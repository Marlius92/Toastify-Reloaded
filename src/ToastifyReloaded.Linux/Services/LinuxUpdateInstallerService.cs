using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using ToastifyReloaded.Linux.Models;

namespace ToastifyReloaded.Linux.Services;

public sealed class LinuxUpdateInstallerService
{
    private readonly ProcessService _process;

    private readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    public LinuxUpdateInstallerService(ProcessService process)
    {
        _process = process;

        _http.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(
                "ToastifyReloaded",
                "1.4.0-preview.4"));
    }

    public async Task<LinuxUpdateApplyResult> DownloadAndApplyAsync(
        LinuxUpdateInfo update,
        CancellationToken cancellationToken = default)
    {
        var kind = DetectInstallKind();
        var arch = RuntimeInformation.ProcessArchitecture;

        var asset = SelectAsset(
            update,
            kind,
            arch);

        if (asset is null)
        {
            return new LinuxUpdateApplyResult(
                false,
                false,
                "Nessun pacchetto compatibile trovato nella release.",
                null);
        }

        var destination =
            await DownloadAsync(
                update,
                asset,
                cancellationToken);

        if (!await VerifyDigestAsync(
                destination,
                asset.Digest,
                cancellationToken))
        {
            File.Delete(destination);

            return new LinuxUpdateApplyResult(
                false,
                false,
                "Verifica SHA-256 del pacchetto non riuscita.",
                null);
        }

        switch (kind)
        {
            case LinuxInstallKind.AppImage:
                return await ApplyAppImageAsync(
                    destination);

            case LinuxInstallKind.Deb:
                return await ApplyDebAsync(
                    destination);

            default:
                return new LinuxUpdateApplyResult(
                    true,
                    false,
                    $"Aggiornamento scaricato: {destination}",
                    destination);
        }
    }

    private LinuxInstallKind DetectInstallKind()
    {
        var appImage =
            Environment.GetEnvironmentVariable("APPIMAGE");

        if (!string.IsNullOrWhiteSpace(appImage) &&
            File.Exists(appImage))
            return LinuxInstallKind.AppImage;

        var processPath =
            Environment.ProcessPath ?? "";

        if (processPath.StartsWith(
                "/opt/toastify-reloaded/",
                StringComparison.Ordinal))
            return LinuxInstallKind.Deb;

        return LinuxInstallKind.Portable;
    }

    private static LinuxUpdateAsset? SelectAsset(
        LinuxUpdateInfo update,
        LinuxInstallKind kind,
        Architecture architecture)
    {
        var isArm64 = architecture == Architecture.Arm64;

        string expected;

        if (kind == LinuxInstallKind.AppImage &&
            !isArm64)
        {
            expected =
                "ToastifyReloaded-Linux-x64.AppImage";
        }
        else if (kind == LinuxInstallKind.Deb)
        {
            expected = isArm64
                ? "arm64.deb"
                : "amd64.deb";
        }
        else
        {
            expected = isArm64
                ? "ToastifyReloaded-Linux-arm64.tar.gz"
                : "ToastifyReloaded-Linux-x64.tar.gz";
        }

        return update.Assets.FirstOrDefault(
            x => kind == LinuxInstallKind.Deb
                ? x.Name.EndsWith(
                    expected,
                    StringComparison.OrdinalIgnoreCase)
                : string.Equals(
                    x.Name,
                    expected,
                    StringComparison.OrdinalIgnoreCase));
    }

    private async Task<string> DownloadAsync(
        LinuxUpdateInfo update,
        LinuxUpdateAsset asset,
        CancellationToken cancellationToken)
    {
        var xdg =
            Environment.GetEnvironmentVariable(
                "XDG_CACHE_HOME");

        var cacheRoot =
            !string.IsNullOrWhiteSpace(xdg)
                ? xdg
                : Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.UserProfile),
                    ".cache");

        var directory = Path.Combine(
            cacheRoot,
            "toastify-reloaded",
            "updates",
            update.Tag);

        Directory.CreateDirectory(directory);

        var destination = Path.Combine(
            directory,
            asset.Name);

        using var response =
            await _http.GetAsync(
                asset.DownloadUri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var input =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        await using var output =
            File.Create(destination);

        await input.CopyToAsync(
            output,
            cancellationToken);

        return destination;
    }

    private static async Task<bool> VerifyDigestAsync(
        string path,
        string? digest,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(digest))
            return true;

        const string prefix = "sha256:";

        if (!digest.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase))
            return true;

        var expected =
            digest[prefix.Length..]
                .Trim()
                .ToLowerInvariant();

        await using var stream =
            File.OpenRead(path);

        var bytes =
            await SHA256.HashDataAsync(
                stream,
                cancellationToken);

        var actual =
            Convert.ToHexString(bytes)
                .ToLowerInvariant();

        return string.Equals(
            expected,
            actual,
            StringComparison.Ordinal);
    }

    private async Task<LinuxUpdateApplyResult> ApplyAppImageAsync(
        string downloadedPath)
    {
        var appImage =
            Environment.GetEnvironmentVariable("APPIMAGE");

        if (string.IsNullOrWhiteSpace(appImage) ||
            !File.Exists(appImage))
        {
            return new LinuxUpdateApplyResult(
                true,
                false,
                $"AppImage scaricata: {downloadedPath}",
                downloadedPath);
        }

        try
        {
            File.SetUnixFileMode(
                downloadedPath,
                UnixFileMode.UserRead |
                UnixFileMode.UserWrite |
                UnixFileMode.UserExecute |
                UnixFileMode.GroupRead |
                UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead |
                UnixFileMode.OtherExecute);

            var backup =
                appImage + ".previous";

            if (File.Exists(backup))
                File.Delete(backup);

            File.Move(
                appImage,
                backup);

            try
            {
                File.Move(
                    downloadedPath,
                    appImage);
            }
            catch
            {
                File.Move(
                    backup,
                    appImage);

                throw;
            }

            File.SetUnixFileMode(
                appImage,
                UnixFileMode.UserRead |
                UnixFileMode.UserWrite |
                UnixFileMode.UserExecute |
                UnixFileMode.GroupRead |
                UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead |
                UnixFileMode.OtherExecute);

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = appImage,
                    UseShellExecute = true
                });

            return new LinuxUpdateApplyResult(
                true,
                true,
                "AppImage aggiornata. Riavvio avviato.",
                appImage);
        }
        catch (Exception ex)
        {
            return new LinuxUpdateApplyResult(
                false,
                false,
                $"Impossibile sostituire AppImage: {ex.Message}",
                downloadedPath);
        }
    }

    private async Task<LinuxUpdateApplyResult> ApplyDebAsync(
        string downloadedPath)
    {
        if (!await _process.ExistsAsync("pkexec") ||
            !await _process.ExistsAsync("apt-get"))
        {
            return new LinuxUpdateApplyResult(
                true,
                false,
                $"Pacchetto .deb scaricato: {downloadedPath}",
                downloadedPath);
        }

        var install =
            await _process.RunAsync(
                "pkexec",
                new[]
                {
                    "apt-get",
                    "install",
                    "-y",
                    downloadedPath
                });

        if (install.ExitCode != 0)
        {
            return new LinuxUpdateApplyResult(
                false,
                false,
                string.IsNullOrWhiteSpace(install.StdErr)
                    ? "Installazione .deb non riuscita."
                    : install.StdErr,
                downloadedPath);
        }

        var launcher = "/usr/bin/toastify-reloaded";

        if (File.Exists(launcher))
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = launcher,
                    UseShellExecute = false
                });

            return new LinuxUpdateApplyResult(
                true,
                true,
                "Pacchetto .deb aggiornato. Riavvio avviato.",
                downloadedPath);
        }

        return new LinuxUpdateApplyResult(
            true,
            false,
            "Pacchetto .deb aggiornato.",
            downloadedPath);
    }

    private enum LinuxInstallKind
    {
        Portable,
        AppImage,
        Deb
    }
}
