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

        settings.TitleFontSize = Math.Clamp(settings.TitleFontSize, 8, 36);
        settings.ArtistFontSize = Math.Clamp(settings.ArtistFontSize, 8, 32);
        settings.TimeFontSize = Math.Clamp(settings.TimeFontSize, 8, 28);

        settings.ToastMarginX = Math.Clamp(settings.ToastMarginX, 0, 500);
        settings.ToastMarginY = Math.Clamp(settings.ToastMarginY, 0, 500);
        settings.MonitorIndex = Math.Max(-1, settings.MonitorIndex);

        settings.ToastPosition = settings.ToastPosition switch
        {
            "TopLeft" => "TopLeft",
            "TopRight" => "TopRight",
            "BottomLeft" => "BottomLeft",
            _ => "BottomRight"
        };

        settings.ToastFontFamily = string.IsNullOrWhiteSpace(settings.ToastFontFamily)
            ? "Inter"
            : settings.ToastFontFamily.Trim();

        settings.CustomTopColor = NormalizeColor(settings.CustomTopColor, "#555555");
        settings.CustomBottomColor = NormalizeColor(settings.CustomBottomColor, "#151515");
        settings.CustomBorderColor = NormalizeColor(settings.CustomBorderColor, "#292929");
        settings.CustomTitleColor = NormalizeColor(settings.CustomTitleColor, "#FFFFFF");
        settings.CustomSecondaryColor = NormalizeColor(settings.CustomSecondaryColor, "#D9D9D9");
        settings.CustomProgressBackgroundColor = NormalizeColor(settings.CustomProgressBackgroundColor, "#252525");
        settings.CustomProgressForegroundColor = NormalizeColor(settings.CustomProgressForegroundColor, "#82B440");

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

    private static string NormalizeColor(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var color = value.Trim();

        if (!color.StartsWith('#'))
            color = "#" + color;

        if (color.Length is not (7 or 9))
            return fallback;

        for (var i = 1; i < color.Length; i++)
        {
            if (!Uri.IsHexDigit(color[i]))
                return fallback;
        }

        return color.ToUpperInvariant();
    }
}
