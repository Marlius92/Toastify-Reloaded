namespace ToastifyReloaded.Linux.Services;

public sealed class LinuxAutostartService
{
    public string AutostartDirectory { get; }
    public string DesktopFilePath { get; }

    public LinuxAutostartService()
    {
        var xdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        var config = !string.IsNullOrWhiteSpace(xdg)
            ? xdg
            : Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config");

        AutostartDirectory = Path.Combine(config, "autostart");
        DesktopFilePath = Path.Combine(
            AutostartDirectory,
            "io.github.Marlius92.ToastifyReloaded.desktop");
    }

    public async Task SetEnabledAsync(bool enabled)
    {
        if (!enabled)
        {
            if (File.Exists(DesktopFilePath))
                File.Delete(DesktopFilePath);
            return;
        }

        Directory.CreateDirectory(AutostartDirectory);

        var executable = Environment.ProcessPath
            ?? throw new InvalidOperationException("Percorso eseguibile non disponibile.");

        var content = $"""
[Desktop Entry]
Type=Application
Name=Toastify Reloaded
Comment=Spotify toast notifications and controls
Exec="{executable}" --background
Icon=io.github.Marlius92.ToastifyReloaded
Terminal=false
Categories=Audio;Utility;
X-GNOME-Autostart-enabled=true
""";

        await File.WriteAllTextAsync(DesktopFilePath, content);
    }
}
