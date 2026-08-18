using System.Text.Json;
using ToastifyReloaded.Mac.Models;

namespace ToastifyReloaded.Mac.Services;

public sealed class MacSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string ConfigDirectory { get; }
    public string SettingsPath { get; }

    public MacSettingsService()
    {
        var overrideDirectory = Environment.GetEnvironmentVariable("TOASTIFY_RELOADED_CONFIG_DIR");
        if (!string.IsNullOrWhiteSpace(overrideDirectory))
        {
            ConfigDirectory = Path.GetFullPath(overrideDirectory);
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            ConfigDirectory = Path.Combine(home, "Library", "Application Support", "ToastifyReloaded");
        }

        SettingsPath = Path.Combine(ConfigDirectory, "settings.json");
    }

    public async Task<MacSettings> LoadAsync()
    {
        Directory.CreateDirectory(ConfigDirectory);

        if (!File.Exists(SettingsPath))
            return new MacSettings();

        try
        {
            await using var stream = File.OpenRead(SettingsPath);
            var settings = await DeserializeCompatibleAsync(stream);
            return Normalize(settings);
        }
        catch
        {
            return new MacSettings();
        }
    }

    public async Task SaveAsync(MacSettings settings)
    {
        Directory.CreateDirectory(ConfigDirectory);
        settings = Normalize(settings);

        var temp = SettingsPath + ".tmp";
        await using (var stream = File.Create(temp))
            await JsonSerializer.SerializeAsync(stream, settings, JsonOptions);

        File.Move(temp, SettingsPath, true);
    }

    public async Task ExportAsync(Stream stream, MacSettings settings)
    {
        settings = Normalize(settings);
        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions);
        await stream.FlushAsync();
    }

    public async Task<MacSettings> ImportAsync(Stream stream)
    {
        var settings = await DeserializeCompatibleAsync(stream);
        return Normalize(settings);
    }

    private static async Task<MacSettings> DeserializeCompatibleAsync(Stream stream)
    {
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        var bytes = memory.ToArray();

        var settings = JsonSerializer.Deserialize<MacSettings>(bytes, JsonOptions)
                       ?? throw new InvalidDataException("Invalid Toastify Reloaded settings file.");

        // Preserve import compatibility with the Linux 1.4 settings JSON.
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;

        var hasX11Hotkeys = TryReadBoolean(root, "EnableX11GlobalHotkeys", out var x11);
        var hasWaylandHotkeys = TryReadBoolean(root, "EnableWaylandPortalHotkeys", out var wayland);
        if (hasX11Hotkeys || hasWaylandHotkeys)
        {
            settings.EnableGlobalHotkeys =
                (hasX11Hotkeys && x11) ||
                (hasWaylandHotkeys && wayland);
        }

        if (TryReadBoolean(root, "AutoCheckLinuxUpdates", out var autoCheck))
            settings.AutoCheckMacUpdates = autoCheck;

        if (TryReadBoolean(root, "AutoInstallLinuxUpdates", out var autoInstall))
            settings.AutoInstallMacUpdates = autoInstall;

        return settings;
    }

    private static bool TryReadBoolean(JsonElement root, string name, out bool value)
    {
        value = false;
        if (!root.TryGetProperty(name, out var node) ||
            node.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            return false;

        value = node.GetBoolean();
        return true;
    }

    internal static MacSettings Normalize(MacSettings settings)
    {
        settings.ApplicationTheme = settings.ApplicationTheme switch
        {
            "Light" => "Light",
            "Dark" => "Dark",
            _ => "System"
        };

        settings.Language = settings.Language == "English" ? "English" : "Italiano";
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
