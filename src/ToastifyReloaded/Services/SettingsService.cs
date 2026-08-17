using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToastifyReloaded.Models;

namespace ToastifyReloaded.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string SettingsDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ToastifyReloaded");

    public string SettingsPath => Path.Combine(SettingsDirectory, "settings.json");

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return Normalize(new AppSettings());

            return LoadFromFile(SettingsPath);
        }
        catch
        {
            return Normalize(new AppSettings());
        }
    }

    public AppSettings LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions)
            ?? throw new InvalidDataException("The settings file does not contain a valid Toastify Reloaded configuration.");
        return Normalize(settings);
    }

    public void Save(AppSettings settings) => SaveToFile(settings, SettingsPath);

    public void SaveToFile(AppSettings settings, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(Normalize(settings), JsonOptions);
        File.WriteAllText(path, json);
    }

    public void Export(AppSettings settings, string destinationPath) =>
        SaveToFile(settings, destinationPath);

    public AppSettings Import(string sourcePath) => LoadFromFile(sourcePath);

    private static AppSettings Normalize(AppSettings settings)
    {
        settings.Hotkeys ??= AppSettings.CreateDefaultHotkeys();
        if (settings.Hotkeys.Count == 0)
            settings.Hotkeys = AppSettings.CreateDefaultHotkeys();

        settings.ApplicationTheme = settings.ApplicationTheme switch
        {
            "Dark" => "Dark",
            "System" => "System",
            _ => "Light"
        };
        settings.ApplicationLanguage = string.Equals(settings.ApplicationLanguage, "Italian", StringComparison.OrdinalIgnoreCase)
            ? "Italian"
            : "English";

        if (string.IsNullOrWhiteSpace(settings.ToastThemePreset))
            settings.ToastThemePreset = ToastThemePresets.FindMatchingName(settings);
        if (string.IsNullOrWhiteSpace(settings.ToastPositionPreset))
            settings.ToastPositionPreset = "BottomRight";
        if (string.IsNullOrWhiteSpace(settings.ToastAnimationStyle))
            settings.ToastAnimationStyle = "Fade";
        if (string.IsNullOrWhiteSpace(settings.ToastAnimationDirection))
            settings.ToastAnimationDirection = "Up";

        settings.ToastMinWidth = Math.Max(150, settings.ToastMinWidth);
        settings.ToastMaxWidth = Math.Max(settings.ToastMinWidth, settings.ToastMaxWidth);
        settings.ToastSlideDistance = Math.Clamp(settings.ToastSlideDistance, 0, 300);
        settings.ToastScreenMargin = Math.Clamp(settings.ToastScreenMargin, 0, 200);

        return settings;
    }
}
