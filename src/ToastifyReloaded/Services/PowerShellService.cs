using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace ToastifyReloaded.Services;

public sealed record PowerShellCommandResult(int ExitCode, string StandardOutput, string StandardError)
{
    public bool Success => ExitCode == 0;
}

public static class PowerShellService
{
    private const string EmbeddedScriptPrefix = "ToastifyReloaded.Scripts.";

    /// <summary>
    /// Estrae in una cartella temporanea uno script PowerShell incorporato
    /// nell'EXE e lo avvia. In questo modo la Release può contenere un solo EXE
    /// pur mantenendo gli helper Lyrics/diagnostica disponibili nell'app.
    /// </summary>
    public static void RunScript(string scriptName, params string[] arguments)
    {
        if (string.IsNullOrWhiteSpace(scriptName) ||
            !string.Equals(scriptName, Path.GetFileName(scriptName), StringComparison.Ordinal))
        {
            throw new ArgumentException("Nome script non valido.", nameof(scriptName));
        }

        var resourceName = EmbeddedScriptPrefix + scriptName;
        var assembly = Assembly.GetExecutingAssembly();
        using var resourceStream = assembly.GetManifestResourceStream(resourceName);
        if (resourceStream is null)
        {
            throw new FileNotFoundException(
                $"Script incorporato non trovato: {scriptName}. La build di Toastify Reloaded potrebbe essere incompleta.");
        }

        var scriptDirectory = Path.Combine(
            Path.GetTempPath(),
            "ToastifyReloaded",
            "embedded-scripts",
            GetAssemblyVersion());
        Directory.CreateDirectory(scriptDirectory);

        var scriptPath = Path.Combine(scriptDirectory, scriptName);
        using (var destination = new FileStream(scriptPath, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            resourceStream.CopyTo(destination);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = true,
            WorkingDirectory = AppContext.BaseDirectory
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        if (Process.Start(startInfo) is null)
            throw new InvalidOperationException("Impossibile avviare PowerShell.");
    }

    public static async Task<PowerShellCommandResult> RunHiddenCommandAsync(
        string command,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(command));
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = AppContext.BaseDirectory
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-EncodedCommand");
        startInfo.ArgumentList.Add(encoded);

        using var process = new Process { StartInfo = startInfo };
        if (!process.Start())
            throw new InvalidOperationException("Impossibile avviare PowerShell.");

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        using var timeoutCts = new CancellationTokenSource(timeout ?? TimeSpan.FromMinutes(4));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        try
        {
            await process.WaitForExitAsync(linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Best effort cleanup only.
            }

            if (cancellationToken.IsCancellationRequested)
                throw;

            return new PowerShellCommandResult(-1, await outputTask, "Comando PowerShell scaduto per timeout.");
        }

        return new PowerShellCommandResult(process.ExitCode, await outputTask, await errorTask);
    }

    private static string GetAssemblyVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version is null
            ? "unknown"
            : $"{version.Major}.{version.Minor}.{Math.Max(version.Build, 0)}";
    }
}
