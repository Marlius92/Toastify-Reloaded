namespace ToastifyReloaded.Linux.Models;

public sealed class LinuxSettings
{
    public string ApplicationTheme { get; set; } = "System";
    public string Language { get; set; } = "Italiano";
    public bool StartWithSession { get; set; } = false;
    public bool CloseToTray { get; set; } = false;

    public bool EnableX11GlobalHotkeys { get; set; } = true;
    public bool EnableWaylandPortalHotkeys { get; set; } = true;

    public string HotkeyPlayPause { get; set; } = "Ctrl+Alt+Space";
    public string HotkeyNext { get; set; } = "Ctrl+Alt+Right";
    public string HotkeyPrevious { get; set; } = "Ctrl+Alt+Left";
    public string HotkeyVolumeUp { get; set; } = "Ctrl+Alt+Up";
    public string HotkeyVolumeDown { get; set; } = "Ctrl+Alt+Down";
    public string HotkeyMute { get; set; } = "Ctrl+Alt+M";
    public string HotkeySeekForward { get; set; } = "Ctrl+Alt+Shift+Right";
    public string HotkeySeekBackward { get; set; } = "Ctrl+Alt+Shift+Left";

    public bool ShowToastOnTrackChange { get; set; } = true;
    public int ToastDisplayMs { get; set; } = 3500;
    public int FadeInMs { get; set; } = 250;
    public int FadeOutMs { get; set; } = 250;

    public bool AutoWidth { get; set; } = true;
    public int MinWidth { get; set; } = 250;
    public int MaxWidth { get; set; } = 600;

    public bool ShowProgress { get; set; } = true;
    public bool ShowSongTime { get; set; } = false;
    public string ImageMode { get; set; } = "Album cover";
    public bool IconFallback { get; set; } = true;

    public string ToastTheme { get; set; } = "Classic Toastify";

    public string ToastFontFamily { get; set; } = "Inter";
    public double TitleFontSize { get; set; } = 15;
    public double ArtistFontSize { get; set; } = 12;
    public double TimeFontSize { get; set; } = 10;

    public string CustomTopColor { get; set; } = "#555555";
    public string CustomBottomColor { get; set; } = "#151515";
    public string CustomBorderColor { get; set; } = "#292929";
    public string CustomTitleColor { get; set; } = "#FFFFFF";
    public string CustomSecondaryColor { get; set; } = "#D9D9D9";
    public string CustomProgressBackgroundColor { get; set; } = "#252525";
    public string CustomProgressForegroundColor { get; set; } = "#82B440";

    public string AnimationStyle { get; set; } = "Fade + Slide";
    public string SlideInDirection { get; set; } = "Up";
    public string SlideOutDirection { get; set; } = "Right";
    public int SlideInDistance { get; set; } = 28;
    public int SlideOutDistance { get; set; } = 50;

    // -1 = primary monitor, 0+ = Screens.All index.
    public int MonitorIndex { get; set; } = -1;
    public string ToastPosition { get; set; } = "BottomRight";
    public int ToastMarginX { get; set; } = 18;
    public int ToastMarginY { get; set; } = 18;

    public bool KeepLyricsPlus { get; set; } = true;

    public bool EnableCompatibilityGuard { get; set; } = true;
    public bool AutoRepairSpicetify { get; set; } = true;
    public string LastSpotifyVersion { get; set; } = "";
    public string LastRepairAttemptVersion { get; set; } = "";

    public bool AutoCheckLinuxUpdates { get; set; } = true;
    public bool AutoInstallLinuxUpdates { get; set; } = false;
}
