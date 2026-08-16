namespace ToastifyReloaded.Models;

public sealed class HotkeyBinding
{
    public HotkeyAction Action { get; set; }
    public string Shortcut { get; set; } = string.Empty;

    public string ActionLabel => Action switch
    {
        HotkeyAction.PlayPause => "Play / Pausa",
        HotkeyAction.NextTrack => "Brano successivo",
        HotkeyAction.PreviousTrack => "Brano precedente",
        HotkeyAction.VolumeUp => "Volume +",
        HotkeyAction.VolumeDown => "Volume -",
        HotkeyAction.Mute => "Mute",
        HotkeyAction.SeekForward => "Avanti 10 secondi",
        HotkeyAction.SeekBackward => "Indietro 10 secondi",
        HotkeyAction.ShowToast => "Mostra popup",
        _ => Action.ToString()
    };
}
