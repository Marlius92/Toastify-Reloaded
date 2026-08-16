namespace ToastifyReloaded.Models;

public sealed class AppSettings
{
    public bool ShowToastOnTrackChange { get; set; } = true;
    public int ToastDurationMs { get; set; } = 3500;
    public bool StartMinimized { get; set; }
    public bool StartWithWindows { get; set; }

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
        new() { Action = HotkeyAction.PlayPause, Shortcut = "Ctrl+Alt+Space" },
        new() { Action = HotkeyAction.NextTrack, Shortcut = "Ctrl+Alt+Right" },
        new() { Action = HotkeyAction.PreviousTrack, Shortcut = "Ctrl+Alt+Left" },
        new() { Action = HotkeyAction.VolumeUp, Shortcut = "Ctrl+Alt+Up" },
        new() { Action = HotkeyAction.VolumeDown, Shortcut = "Ctrl+Alt+Down" },
        new() { Action = HotkeyAction.Mute, Shortcut = "Ctrl+Alt+M" },
        new() { Action = HotkeyAction.SeekForward, Shortcut = "Ctrl+Alt+Shift+Right" },
        new() { Action = HotkeyAction.SeekBackward, Shortcut = "Ctrl+Alt+Shift+Left" },
        new() { Action = HotkeyAction.ShowToast, Shortcut = "Ctrl+Alt+T" }
    ];
}
