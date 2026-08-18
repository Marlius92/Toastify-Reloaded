using System.Text.Json;
using ToastifyReloaded.Linux.Models;

namespace ToastifyReloaded.Linux.Services;

public sealed class LinuxSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string ConfigDirectory { get; }
    public string SettingsPath { get; }

    public LinuxSettingsService()
    {
        var xdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        ConfigDirectory = !string.IsNullOrWhiteSpace(xdg)
            ? Path.Combine(xdg, "toastify-reloaded")
            : Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config",
                "toastify-reloaded");

        SettingsPath = Path.Combine(ConfigDirectory, "settings.json");
    }

    public async Task<LinuxSettings> LoadAsync()
    {
        Directory.CreateDirectory(ConfigDirectory);

        if (!File.Exists(SettingsPath))
            return new LinuxSettings();

        try
        {
            await using var stream = File.OpenRead(SettingsPath);
            return await JsonSerializer.DeserializeAsync<LinuxSettings>(stream, JsonOptions)
                   ?? new LinuxSettings();
        }
        catch
        {
            return new LinuxSettings();
        }
    }

    public async Task SaveAsync(LinuxSettings settings)
    {
        Directory.CreateDirectory(ConfigDirectory);
        var temp = SettingsPath + ".tmp";

        await using (var stream = File.Create(temp))
            await JsonSerializer.SerializeAsync(stream, settings, JsonOptions);

        File.Move(temp, SettingsPath, true);
    }
}
