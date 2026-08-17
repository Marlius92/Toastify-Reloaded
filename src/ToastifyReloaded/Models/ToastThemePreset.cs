namespace ToastifyReloaded.Models;

public sealed record ToastThemePreset(
    string Name,
    string Description,
    string TopColor,
    string BottomColor,
    string BorderColor,
    string Title1Color,
    string Title2Color,
    string ProgressBackground,
    string ProgressForeground,
    double BorderThickness = 1,
    double CornerRadius = 4,
    bool Title1Shadow = false,
    bool Title2Shadow = false)
{
    public void ApplyTo(AppSettings settings)
    {
        settings.ToastThemePreset = Name;
        settings.ToastColorTop = TopColor;
        settings.ToastColorBottom = BottomColor;
        settings.ToastColorTopOffset = 0;
        settings.ToastColorBottomOffset = 1;
        settings.ToastBorderColor = BorderColor;
        settings.ToastTitle1Color = Title1Color;
        settings.ToastTitle2Color = Title2Color;
        settings.SongProgressBarBackgroundColor = ProgressBackground;
        settings.SongProgressBarForegroundColor = ProgressForeground;
        settings.ToastBorderThickness = BorderThickness;
        settings.ToastCornerTopLeft = CornerRadius;
        settings.ToastCornerTopRight = CornerRadius;
        settings.ToastCornerBottomLeft = CornerRadius;
        settings.ToastCornerBottomRight = CornerRadius;
        settings.ToastTitle1DropShadow = Title1Shadow;
        settings.ToastTitle2DropShadow = Title2Shadow;
    }
}

public static class ToastThemePresets
{
    public const string CustomName = "Custom";

    public static IReadOnlyList<ToastThemePreset> All { get; } =
    [
        new(
            "Classic Toastify",
            "The original Toastify gray-to-black look.",
            "#FF555555", "#FF151515", "#FF292929",
            "#FFFFFFFF", "#FFF0F0F0", "#FF333333", "#FFA0A0A0",
            1, 4),
        new(
            "Spotify Green",
            "Charcoal surfaces with Spotify-style green accents.",
            "#FF26312A", "#FF101512", "#FF1DB954",
            "#FFFFFFFF", "#FFD9F4E3", "#FF26332B", "#FF1DB954",
            1, 6, true, false),
        new(
            "Midnight Blue",
            "Deep navy with a bright electric-blue border.",
            "#FF17223A", "#FF090F1D", "#FF3B82F6",
            "#FFF4F8FF", "#FFC7D7F8", "#FF15233A", "#FF60A5FA",
            1, 6, true, false),
        new(
            "Neon Purple",
            "Dark violet surfaces with luminous purple accents.",
            "#FF2B183E", "#FF100918", "#FFB65CFF",
            "#FFFFFFFF", "#FFEAD7FF", "#FF241331", "#FFB65CFF",
            1, 7, true, true),
        new(
            "Cyberpunk",
            "Near-black blue with cyan and hot-magenta highlights.",
            "#FF15182A", "#FF070910", "#FFFF2BD6",
            "#FF70F8FF", "#FFFF9CE9", "#FF172238", "#FF00E5FF",
            1.5, 5, true, true),
        new(
            "Crimson Night",
            "Black-red gradient with a crisp crimson edge.",
            "#FF32171A", "#FF120708", "#FFE53935",
            "#FFFFFFFF", "#FFFFD0D0", "#FF311214", "#FFE53935",
            1, 5, true, false),
        new(
            "Amber Gold",
            "Warm charcoal and amber for a refined gold appearance.",
            "#FF3A2B12", "#FF151006", "#FFFFC857",
            "#FFFFF7E1", "#FFFFDF9A", "#FF33250F", "#FFFFC857",
            1, 6, true, false),
        new(
            "Emerald",
            "Forest green with a saturated emerald accent.",
            "#FF12332B", "#FF071612", "#FF24D18F",
            "#FFF4FFF9", "#FFC7F6E4", "#FF123127", "#FF24D18F",
            1, 6, true, false),
        new(
            "Ocean",
            "Deep ocean blue with a turquoise progress bar.",
            "#FF123646", "#FF07151C", "#FF22C7E8",
            "#FFF1FCFF", "#FFBCECF5", "#FF10303C", "#FF22C7E8",
            1, 6, true, false),
        new(
            "Sakura",
            "Muted plum with soft cherry-blossom pink accents.",
            "#FF392331", "#FF160D13", "#FFFF86B7",
            "#FFFFF5FA", "#FFFFD1E4", "#FF321D2A", "#FFFF86B7",
            1, 8, true, false),
        new(
            "Arctic",
            "A light icy preset with dark text and cold-blue accents.",
            "#FFEAF4FA", "#FFC8D9E6", "#FF5B85A6",
            "#FF102030", "#FF26394B", "#FFB6C9D7", "#FF4A90C2",
            1, 5),
        new(
            "Monochrome",
            "Pure grayscale: black, white and neutral silver.",
            "#FF303030", "#FF090909", "#FFB8B8B8",
            "#FFFFFFFF", "#FFDADADA", "#FF2A2A2A", "#FFE8E8E8",
            1, 4),
        new(
            "Retro Synthwave",
            "Purple night tones with magenta, cyan and sunset orange.",
            "#FF2A1645", "#FF0B0614", "#FFFF4ECD",
            "#FFFFE6F8", "#FF70E6FF", "#FF281238", "#FFFFB14E",
            1.5, 7, true, true)
    ];

    public static ToastThemePreset? Find(string? name) =>
        All.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public static string FindMatchingName(AppSettings settings)
    {
        foreach (var preset in All)
        {
            if (Matches(settings, preset))
                return preset.Name;
        }
        return CustomName;
    }

    public static bool Matches(AppSettings settings, ToastThemePreset preset) =>
        Same(settings.ToastColorTop, preset.TopColor) &&
        Same(settings.ToastColorBottom, preset.BottomColor) &&
        Same(settings.ToastBorderColor, preset.BorderColor) &&
        Same(settings.ToastTitle1Color, preset.Title1Color) &&
        Same(settings.ToastTitle2Color, preset.Title2Color) &&
        Same(settings.SongProgressBarBackgroundColor, preset.ProgressBackground) &&
        Same(settings.SongProgressBarForegroundColor, preset.ProgressForeground) &&
        Nearly(settings.ToastBorderThickness, preset.BorderThickness) &&
        Nearly(settings.ToastCornerTopLeft, preset.CornerRadius) &&
        Nearly(settings.ToastCornerTopRight, preset.CornerRadius) &&
        Nearly(settings.ToastCornerBottomLeft, preset.CornerRadius) &&
        Nearly(settings.ToastCornerBottomRight, preset.CornerRadius) &&
        settings.ToastTitle1DropShadow == preset.Title1Shadow &&
        settings.ToastTitle2DropShadow == preset.Title2Shadow;

    private static bool Same(string? a, string? b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static bool Nearly(double a, double b) => Math.Abs(a - b) < 0.001;
}
