using System.Diagnostics;
using System.IO;

namespace ToastifyReloaded.Services;

public static class PowerShellService
{
    public static void RunScript(string scriptName, params string[] arguments)
    {
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "scripts", scriptName);
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException("Script non trovato nella cartella di Toastify Reloaded.", scriptPath);

        // Current bundled arguments are PowerShell switches without spaces.
        // Keeping them separate from scriptPath avoids quoting the path incorrectly.
        var extraArguments = arguments.Length == 0 ? string.Empty : " " + string.Join(" ", arguments);
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"{extraArguments}",
            UseShellExecute = true,
            WorkingDirectory = AppContext.BaseDirectory
        };

        Process.Start(startInfo);
    }
}
