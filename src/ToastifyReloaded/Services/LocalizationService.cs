namespace ToastifyReloaded.Services;

public static class LocalizationService
{
    public const string English = "English";
    public const string Italian = "Italian";

    private static readonly IReadOnlyDictionary<string, string> It = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["General"] = "Generale",
        ["Hotkeys"] = "Scorciatoie",
        ["Toast"] = "Toast",
        ["Advanced"] = "Avanzate",
        ["Reloaded"] = "Reloaded",
        ["ColorsFont"] = "Colori e font",
        ["Themes"] = "Temi",
        ["Animations"] = "Animazioni",
        ["Position"] = "Posizione",
        ["Save"] = "Salva",
        ["Default"] = "Predefiniti",
        ["Appearance"] = "Aspetto",
        ["ApplicationTheme"] = "Tema applicazione:",
        ["Language"] = "Lingua interfaccia:",
        ["ThemeLight"] = "Chiaro",
        ["ThemeDark"] = "Scuro",
        ["ThemeSystem"] = "Segui Windows",
        ["SettingsBackup"] = "Importazione / esportazione impostazioni",
        ["Diagnostics"] = "Diagnostica e compatibilità",
        ["Integrations"] = "Integrazioni avanzate",
        ["ExportSettings"] = "Esporta impostazioni",
        ["ImportSettings"] = "Importa impostazioni",
        ["CopyReport"] = "Copia rapporto",
        ["ExportReport"] = "Esporta rapporto",
        ["LyricsPlus"] = "Lyrics Plus",
        ["UpdatesCompatibility"] = "Aggiornamenti e Compatibility Guard",
        ["Tools"] = "Strumenti",
        ["ApplyPreset"] = "Applica preset",
        ["TestToast"] = "Test Toast",
        ["Preset"] = "Preset tema:",
        ["AnimationStyle"] = "Animazione:",
        ["AnimationDirection"] = "Direzione:",
        ["SlideDistance"] = "Distanza slide:",
        ["FadeIn"] = "Fade In:",
        ["FadeOut"] = "Fade Out:",
        ["Monitor"] = "Monitor:",
        ["PositionPreset"] = "Preset posizione:",
        ["ScreenMargin"] = "Margine schermo:",
        ["CustomCoordinates"] = "Coordinate personalizzate",
        ["Preview"] = "Anteprima"
    };

    public static string Get(string? language, string key, string english)
    {
        if (string.Equals(language, Italian, StringComparison.OrdinalIgnoreCase) && It.TryGetValue(key, out var translated))
            return translated;
        return english;
    }
}
