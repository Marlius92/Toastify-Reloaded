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
            var settings = await JsonSerializer.DeserializeAsync<LinuxSettings>(
                stream,
                JsonOptions);

            return Normalize(settings ?? new LinuxSettings());
        }
        catch
        {
            return new LinuxSettings();
        }
    }

    public async Task SaveAsync(LinuxSettings settings)
    {
        Directory.CreateDirectory(ConfigDirectory);
        settings = Normalize(settings);

        var temp = SettingsPath + ".tmp";

        await using (var stream = File.Create(temp))
            await JsonSerializer.SerializeAsync(stream, settings, JsonOptions);

        File.Move(temp, SettingsPath, true);
    }

    public async Task ExportAsync(Stream stream, LinuxSettings settings)
    {
        settings = Normalize(settings);
        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions);
        await stream.FlushAsync();
    }

    public async Task<LinuxSettings> ImportAsync(Stream stream)
    {
        var settings = await JsonSerializer.DeserializeAsync<LinuxSettings>(
            stream,
            JsonOptions);

        if (settings is null)
            throw new InvalidDataException("Invalid Toastify Reloaded settings file.");

        return Normalize(settings);
    }

    private static LinuxSettings Normalize(LinuxSettings settings)
    {
        settings.ApplicationTheme = settings.ApplicationTheme switch
        {
            "Light" => "Light",
            "Dark" => "Dark",
            _ => "System"
        };

        settings.Language = settings.Language == "English"
            ? "English"
            : "Italiano";

        settings.ToastDisplayMs = Math.Clamp(settings.ToastDisplayMs, 500, 15000);
        settings.FadeInMs = Math.Clamp(settings.FadeInMs, 0, 5000);
        settings.FadeOutMs = Math.Clamp(settings.FadeOutMs, 0, 5000);

        settings.MinWidth = Math.Clamp(settings.MinWidth, 220, 1000);
        settings.MaxWidth = Math.Clamp(settings.MaxWidth, settings.MinWidth, 1400);

        settings.SlideInDistance = Math.Clamp(settings.SlideInDistance, 0, 300);
        settings.SlideOutDistance = Math.Clamp(settings.SlideOutDistance, 0, 300);

        settings.HotkeyPlayPause ??= "";
        settings.HotkeyNext ??= "";
        settings.HotkeyPrevious ??= "";
        settings.HotkeyVolumeUp ??= "";
        settings.HotkeyVolumeDown ??= "";
        settings.HotkeyMute ??= "";
        settings.HotkeySeekForward ??= "";
        settings.HotkeySeekBackward ??= "";
        settings.LastSpotifyVersion ??= "";
        settings.LastRepairAttemptVersion ??= "";

        return settings;
    }
}
