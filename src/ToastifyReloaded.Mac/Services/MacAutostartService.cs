using System.Security;

namespace ToastifyReloaded.Mac.Services;

public sealed class MacAutostartService
{
    private const string Label = "io.github.Marlius92.ToastifyReloaded";
    private readonly ProcessService _process = new();

    private static string LaunchAgentsDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Library",
        "LaunchAgents");

    public string PlistPath => Path.Combine(LaunchAgentsDirectory, Label + ".plist");

    public async Task SetEnabledAsync(bool enabled)
    {
        if (!OperatingSystem.IsMacOS())
            return;

        Directory.CreateDirectory(LaunchAgentsDirectory);

        var uid = await GetUidAsync();
        if (uid is null)
            return;

        if (!enabled)
        {
            await BootoutAsync(uid);
            if (File.Exists(PlistPath))
                File.Delete(PlistPath);
            return;
        }

        var executable = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(executable) || !File.Exists(executable))
            return;

        var escaped = SecurityElement.Escape(executable) ?? executable;
        var plist = string.Join("\n", new[]
        {
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
            "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">",
            "<plist version=\"1.0\">",
            "<dict>",
            "  <key>Label</key>",
            $"  <string>{Label}</string>",
            "  <key>ProgramArguments</key>",
            "  <array>",
            $"    <string>{escaped}</string>",
            "  </array>",
            "  <key>RunAtLoad</key>",
            "  <true/>",
            "  <key>KeepAlive</key>",
            "  <false/>",
            "  <key>ProcessType</key>",
            "  <string>Interactive</string>",
            "</dict>",
            "</plist>",
            string.Empty
        });

        await File.WriteAllTextAsync(PlistPath, plist);
        await BootoutAsync(uid);
        _ = await _process.RunAsync(
            "launchctl",
            new[] { "bootstrap", $"gui/{uid}", PlistPath });
    }

    private async Task BootoutAsync(string uid)
    {
        try
        {
            _ = await _process.RunAsync(
                "launchctl",
                new[] { "bootout", $"gui/{uid}", PlistPath });
        }
        catch
        {
        }
    }

    private async Task<string?> GetUidAsync()
    {
        try
        {
            var result = await _process.RunAsync("id", new[] { "-u" });
            return result.ExitCode == 0 && !string.IsNullOrWhiteSpace(result.StdOut)
                ? result.StdOut.Trim()
                : null;
        }
        catch
        {
            return null;
        }
    }
}
