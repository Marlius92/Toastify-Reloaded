namespace ToastifyReloaded.Models;

public sealed class AppSettings
{
    // Original Toastify-style general settings.
    public bool StartWithWindows { get; set; }
    public bool MinimizeSpotifyOnStartup { get; set; }
    public bool CloseSpotifyWithToastify { get; set; }
    public string VolumeControlMode { get; set; } = "Windows Volume Mixer";
    public double WindowsVolumeMixerIncrement { get; set; } = 1.0;
    public string ClipboardTemplate { get; set; } = "{0}";
    public bool SaveTrackToFile { get; set; }
    public string TrackFilePath { get; set; } = string.Empty;
    public bool GlobalHotkeysEnabled { get; set; } = true;
    public bool OptInToAnalytics { get; set; }

    // Toast settings. Defaults reproduce the Toastify 1.11.2 popup geometry.
    public bool ShowToastOnTrackChange { get; set; } = true;
    public bool OnlyShowToastOnHotkey { get; set; }
    public bool DisableToastWithFullscreenApps { get; set; }
    public bool ShowSongProgressBar { get; set; } = true;
    public int ToastDurationMs { get; set; } = 3500;
    public string ToastTitlesOrder { get; set; } = "TrackArtist";
    public double ToastWidth { get; set; } = 250;
    public double ToastHeight { get; set; } = 70;
    public double PositionLeft { get; set; } = -1;
    public double PositionTop { get; set; } = -1;
    public double ToastBorderThickness { get; set; } = 1;
    public double ToastCornerTopLeft { get; set; } = 4;
    public double ToastCornerTopRight { get; set; } = 4;
    public double ToastCornerBottomLeft { get; set; } = 4;
    public double ToastCornerBottomRight { get; set; } = 4;
    public string ToastColorTop { get; set; } = "#FF555555";
    public string ToastColorBottom { get; set; } = "#FF151515";
    public double ToastColorTopOffset { get; set; } = 0;
    public double ToastColorBottomOffset { get; set; } = 1;
    public string ToastBorderColor { get; set; } = "#FF292929";
    public string ToastTitle1Color { get; set; } = "#FFFFFFFF";
    public string ToastTitle2Color { get; set; } = "#FFF0F0F0";
    public double ToastTitle1FontSize { get; set; } = 16;
    public double ToastTitle2FontSize { get; set; } = 12;
    public bool ToastTitle1DropShadow { get; set; }
    public bool ToastTitle2DropShadow { get; set; }
    public double ToastTitle1ShadowDepth { get; set; } = 3;
    public double ToastTitle1ShadowBlur { get; set; } = 2;
    public double ToastTitle2ShadowDepth { get; set; } = 3;
    public double ToastTitle2ShadowBlur { get; set; } = 2;
    public string SongProgressBarBackgroundColor { get; set; } = "#FF333333";
    public string SongProgressBarForegroundColor { get; set; } = "#FFA0A0A0";

    // Original Advanced-tab values are persisted even when not used by the
    // modern GSMTC playback backend.
    public bool UseProxy { get; set; }
    public string ProxyHost { get; set; } = string.Empty;
    public string ProxyPort { get; set; } = string.Empty;
    public string ProxyUsername { get; set; } = string.Empty;
    public bool BypassProxyOnLocal { get; set; }
    public bool EnableSpotifyWebApi { get; set; }
    public bool EnableBroadcaster { get; set; }

    // Start/tray behavior retained from Reloaded.
    public bool StartMinimized { get; set; }

    // Automatic maintenance / compatibility guard.
    public bool AutoCheckToastifyUpdates { get; set; } = true;
    public bool AutoInstallToastifyUpdates { get; set; } = true;
    public bool AutoRepairAfterSpotifyUpdate { get; set; } = true;
    public bool KeepLyricsPlusEnabled { get; set; } = true;
    public bool AutoUpgradeSpicetify { get; set; } = true;
    public bool RestartSpotifyAfterRepair { get; set; } = true;

    // Internal state used to detect Spotify upgrades without repair loops.
    public string LastKnownSpotifyVersion { get; set; } = string.Empty;
    public string LastAutoRepairAttemptVersion { get; set; } = string.Empty;
    public DateTimeOffset? LastAutoRepairAttemptUtc { get; set; }

    public List<HotkeyBinding> Hotkeys { get; set; } = CreateDefaultHotkeys();

    public static List<HotkeyBinding> CreateDefaultHotkeys() =>
    [
        new() { Action = HotkeyAction.PlayPause, Shortcut = "Ctrl+Alt+Space", Enabled = true },
        new() { Action = HotkeyAction.NextTrack, Shortcut = "Ctrl+Alt+Right", Enabled = true },
        new() { Action = HotkeyAction.PreviousTrack, Shortcut = "Ctrl+Alt+Left", Enabled = true },
        new() { Action = HotkeyAction.VolumeUp, Shortcut = "Ctrl+Alt+Up", Enabled = true },
        new() { Action = HotkeyAction.VolumeDown, Shortcut = "Ctrl+Alt+Down", Enabled = true },
        new() { Action = HotkeyAction.Mute, Shortcut = "Ctrl+Alt+M", Enabled = true },
        new() { Action = HotkeyAction.SeekForward, Shortcut = "Ctrl+Alt+Shift+Right", Enabled = true },
        new() { Action = HotkeyAction.SeekBackward, Shortcut = "Ctrl+Alt+Shift+Left", Enabled = true },
        new() { Action = HotkeyAction.ShowToast, Shortcut = "Ctrl+Alt+T", Enabled = true }
    ];
}
