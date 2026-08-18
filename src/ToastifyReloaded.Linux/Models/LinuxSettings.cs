namespace ToastifyReloaded.Linux.Models;

public sealed class LinuxSettings
{
    public string ApplicationTheme { get; set; } = "System";
    public string Language { get; set; } = "Italiano";
    public bool StartWithSession { get; set; } = false;
    public bool EnableX11GlobalHotkeys { get; set; } = true;

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

    public string AnimationStyle { get; set; } = "Fade + Slide";
    public string SlideInDirection { get; set; } = "Up";
    public string SlideOutDirection { get; set; } = "Right";
    public int SlideInDistance { get; set; } = 28;
    public int SlideOutDistance { get; set; } = 50;

    public bool KeepLyricsPlus { get; set; } = true;
}
