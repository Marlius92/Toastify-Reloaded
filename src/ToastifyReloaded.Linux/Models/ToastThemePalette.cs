namespace ToastifyReloaded.Linux.Models;

public sealed record ToastThemePalette(
    string Top,
    string Bottom,
    string Border,
    string Title,
    string Secondary,
    string ProgressBackground,
    string ProgressForeground)
{
    public static ToastThemePalette FromSettings(LinuxSettings settings)
    {
        if (settings.ToastTheme == "Custom")
        {
            return new ToastThemePalette(
                settings.CustomTopColor,
                settings.CustomBottomColor,
                settings.CustomBorderColor,
                settings.CustomTitleColor,
                settings.CustomSecondaryColor,
                settings.CustomProgressBackgroundColor,
                settings.CustomProgressForegroundColor);
        }

        return FromName(settings.ToastTheme);
    }

    public static ToastThemePalette FromName(string name)
        => name switch
        {
            "Spotify Green" => new("#202020", "#0F0F0F", "#1DB954", "#FFFFFF", "#D8D8D8", "#303030", "#1DB954"),
            "Midnight Blue" => new("#172033", "#080D18", "#3E7BFA", "#FFFFFF", "#C7D6FF", "#202A3A", "#4D8BFF"),
            "Neon Purple" => new("#261736", "#0C0712", "#B85CFF", "#FFFFFF", "#E4C8FF", "#332042", "#C56DFF"),
            "Cyberpunk" => new("#17152F", "#070714", "#FF3CAC", "#FFFFFF", "#7AF7FF", "#252044", "#00E5FF"),
            "Crimson Night" => new("#2D151A", "#0D0709", "#D92D4B", "#FFFFFF", "#F3C5CD", "#3A1A20", "#EF476F"),
            "Amber Gold" => new("#2A2418", "#0E0B06", "#E6B84A", "#FFF8E6", "#DCCFA9", "#3A311E", "#F2C75C"),
            "Emerald" => new("#13261F", "#07110D", "#23C483", "#FFFFFF", "#BFE8D8", "#18352A", "#2EE59D"),
            "Ocean" => new("#122835", "#061017", "#26C6DA", "#FFFFFF", "#C2EDF3", "#163440", "#35D7E8"),
            "Sakura" => new("#2B1B2D", "#100811", "#F28FB8", "#FFFFFF", "#F2D1E0", "#3A233B", "#FF9CC4"),
            "Arctic" => new("#26313A", "#10161C", "#9DD9F3", "#FFFFFF", "#DCECF4", "#35434D", "#B9E8FA"),
            "Monochrome" => new("#2A2A2A", "#0B0B0B", "#B0B0B0", "#FFFFFF", "#D0D0D0", "#333333", "#FFFFFF"),
            "Retro Synthwave" => new("#24123B", "#090411", "#FF5EA8", "#FFFFFF", "#FFB05A", "#32194E", "#FF7A59"),
            _ => new("#555555", "#151515", "#292929", "#FFFFFF", "#D9D9D9", "#252525", "#82B440")
        };
}
