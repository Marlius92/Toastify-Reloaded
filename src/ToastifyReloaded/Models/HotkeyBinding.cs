namespace ToastifyReloaded.Models;

public sealed class HotkeyBinding
{
    public HotkeyAction Action { get; set; }
    public string Shortcut { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;

    public string ActionLabel => Action switch
    {
        HotkeyAction.PlayPause => "Play/Pause",
        HotkeyAction.NextTrack => "Next Track",
        HotkeyAction.PreviousTrack => "Previous Track",
        HotkeyAction.VolumeUp => "Volume Up",
        HotkeyAction.VolumeDown => "Volume Down",
        HotkeyAction.Mute => "Mute",
        HotkeyAction.SeekForward => "Seek Forward",
        HotkeyAction.SeekBackward => "Seek Backward",
        HotkeyAction.ShowToast => "Show Toast",
        _ => Action.ToString()
    };
}
