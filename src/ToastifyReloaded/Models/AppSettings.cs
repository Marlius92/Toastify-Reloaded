namespace ToastifyReloaded.Models;

public sealed class AppSettings
{
    public bool ShowToastOnTrackChange { get; set; } = true;
    public int ToastDurationMs { get; set; } = 3500;
    public bool StartMinimized { get; set; }
    public bool StartWithWindows { get; set; }

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
