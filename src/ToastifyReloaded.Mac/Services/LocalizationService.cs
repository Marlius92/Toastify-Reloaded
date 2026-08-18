using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;

namespace ToastifyReloaded.Mac.Services;

public sealed class LocalizationService
{
    private sealed record Entry(string Key, string It, string En);

    private static readonly Entry[] Entries =
    {
        new("General", "Generale", "General"),

        new("ColorsFont", "Colori e font", "Colors & Font"),
        new("FontFamily", "Font", "Font"),
        new("TitleSize", "Dimensione titolo", "Title size"),
        new("ArtistSize", "Dimensione artista", "Artist size"),
        new("TimeSize", "Dimensione tempo", "Time size"),
        new("CustomColors", "Colori Custom", "Custom colors"),
        new("TopColor", "Colore superiore", "Top color"),
        new("BottomColor", "Colore inferiore", "Bottom color"),
        new("BorderColor", "Bordo", "Border"),
        new("TitleColor", "Titolo", "Title"),
        new("SecondaryColor", "Testo secondario", "Secondary text"),
        new("ProgressBackground", "Sfondo avanzamento", "Progress background"),
        new("ProgressForeground", "Avanzamento", "Progress foreground"),
        new("Position", "Posizione", "Position"),
        new("Monitor", "Monitor", "Monitor"),
        new("PrimaryMonitor", "Monitor principale", "Primary monitor"),
        new("ToastPosition", "Posizione toast", "Toast position"),
        new("TopLeft", "Alto sinistra", "Top left"),
        new("TopRight", "Alto destra", "Top right"),
        new("BottomLeft", "Basso sinistra", "Bottom left"),
        new("BottomRight", "Basso destra", "Bottom right"),
        new("HorizontalMargin", "Margine orizzontale", "Horizontal margin"),
        new("VerticalMargin", "Margine verticale", "Vertical margin"),
        new("InstallUpdates", "Installa automaticamente gli aggiornamenti macOS", "Automatically install macOS updates"),
        new("DownloadInstall", "Scarica / installa aggiornamento", "Download / install update"),
        new("Custom", "Custom", "Custom"),
        new("Hotkeys", "Hotkeys", "Hotkeys"),
        new("Advanced", "Avanzate", "Advanced"),
        new("Reloaded", "Reloaded", "Reloaded"),
        new("Interface", "Interfaccia", "Interface"),
        new("ApplicationTheme", "Tema applicazione", "Application theme"),
        new("Language", "Lingua", "Language"),
        new("Autostart", "Avvia Toastify Reloaded all’accesso a macOS", "Start Toastify Reloaded when you log in to macOS"),
        new("CloseToTray", "Chiudi la finestra nell'area di notifica", "Close the window to the system tray"),
        new("RefreshStatus", "Aggiorna stato", "Refresh status"),
        new("EnableHotkeys", "Abilita hotkey globali", "Enable global hotkeys"),
        new("NextTrack", "Brano successivo", "Next track"),
        new("PreviousTrack", "Brano precedente", "Previous track"),
        new("Behavior", "Comportamento", "Behavior"),
        new("ShowToast", "Mostra toast al cambio brano", "Show toast when the track changes"),
        new("ToastDuration", "Durata toast (ms)", "Toast duration (ms)"),
        new("ShowProgress", "Mostra barra avanzamento", "Show progress bar"),
        new("ShowTime", "Mostra tempo / durata", "Show time / duration"),
        new("SizeArtwork", "Dimensione e copertina", "Size and artwork"),
        new("AutoWidth", "Adatta larghezza a titolo / artista", "Adapt width to title / artist"),
        new("MinWidth", "Larghezza min", "Minimum width"),
        new("MaxWidth", "Larghezza max", "Maximum width"),
        new("Image", "Immagine", "Image"),
        new("IconFallback", "Usa icona Toastify se la cover non è disponibile", "Use Toastify icon when artwork is unavailable"),
        new("Themes", "Temi", "Themes"),
        new("ToastTheme", "Tema toast", "Toast theme"),
        new("TestToast", "Mostra toast di test", "Show test toast"),
        new("Animations", "Animazioni", "Animations"),
        new("AnimationStyle", "Stile animazione", "Animation style"),
        new("SlideInDistance", "Distanza entrata", "Entrance distance"),
        new("SlideOutDistance", "Distanza uscita", "Exit distance"),
        new("Diagnostics", "Diagnostica macOS", "macOS diagnostics"),
        new("RefreshDiagnostics", "Aggiorna diagnostica", "Refresh diagnostics"),
        new("PreviewNotes", "Note macOS", "macOS notes"),
        new("Compatibility", "Compatibilità macOS", "macOS compatibility"),
        new("EnableLyrics", "Abilita Lyrics Plus", "Enable Lyrics Plus"),
        new("Repair", "Ripara Spotify / Lyrics", "Repair Spotify / Lyrics"),
        new("Save", "Salva", "Save"),
        new("Exit", "Esci", "Exit"),
        new("SettingsIO", "Import / Export impostazioni", "Settings import / export"),
        new("ExportSettings", "Esporta impostazioni", "Export settings"),
        new("ImportSettings", "Importa impostazioni", "Import settings"),
        new("Updates", "Aggiornamenti macOS", "macOS updates"),
        new("AutoCheckUpdates", "Controlla automaticamente aggiornamenti macOS", "Automatically check for macOS updates"),
        new("CheckUpdates", "Controlla aggiornamenti", "Check for updates"),
        new("OpenRelease", "Apri ultima release", "Open latest release"),
        new("Guard", "Compatibility Guard macOS", "macOS Compatibility Guard"),
        new("EnableGuard", "Abilita Compatibility Guard", "Enable Compatibility Guard"),
        new("AutoRepair", "Ripara automaticamente Spicetify dopo un aggiornamento Spotify", "Automatically repair Spicetify after a Spotify update"),
        new("RunGuard", "Esegui controllo compatibilità", "Run compatibility check"),
        new("System", "Sistema", "System"),
        new("Light", "Chiaro", "Light"),
        new("Dark", "Scuro", "Dark"),
        new("ItalianLanguage", "Italiano", "Italian"),
        new("EnglishLanguage", "Inglese", "English"),
        new("AlbumCover", "Copertina album", "Album cover"),
        new("None", "Nessuna", "None"),
        new("Up", "Su", "Up"),
        new("Down", "Giù", "Down"),
        new("Left", "Sinistra", "Left"),
        new("Right", "Destra", "Right"),
        new("TrayOpen", "Apri impostazioni", "Open settings"),
        new("TrayPlayPause", "Play / Pausa", "Play / Pause"),
        new("TrayNext", "Brano successivo", "Next track"),
        new("TrayPrevious", "Brano precedente", "Previous track"),
        new("TrayExit", "Esci", "Exit"),
        new("SpotifyBackendInfo", "I controlli Spotify usano AppleScript. Le hotkey globali usano il backend nativo macOS e richiedono il permesso Accessibilità.", "Spotify controls use AppleScript. Global hotkeys use the native macOS backend and require Accessibility permission."),
        new("MacHotkeyHelp", "macOS: al primo utilizzo delle hotkey può essere richiesto il permesso in Impostazioni di Sistema → Privacy e Sicurezza → Accessibilità.", "macOS: the first time global hotkeys are used, grant permission in System Settings → Privacy & Security → Accessibility."),
        new("MacPreviewNote", "Toastify Reloaded macOS 1.5.0 Preview 1: stessa interfaccia, impostazioni e funzioni della linea stabile, con backend macOS dedicati per Spotify, hotkey, autostart e aggiornamenti.", "Toastify Reloaded macOS 1.5.0 Preview 1: the same interface, settings and features as the stable line, with dedicated macOS backends for Spotify, hotkeys, autostart and updates."),
        new("MacSpotifyInfo", "Spotify viene controllato tramite AppleScript senza dipendere dalla finestra interna di Spotify. macOS può chiedere il permesso Automazione al primo controllo.", "Spotify is controlled through AppleScript without depending on Spotify's internal window. macOS may request Automation permission on first control."),
        new("Granted", "concesso", "granted"),
        new("Required", "richiesto", "required"),
        new("HotkeysReady", "hotkey globali attive", "global hotkeys active"),
        new("HotkeysDisabled", "hotkey globali disattivate", "global hotkeys disabled"),
        new("AccessibilityRequired", "permesso Accessibilità richiesto", "Accessibility permission required"),
        new("SpotifyReady", "Spotify pronto", "Spotify ready"),
        new("SpotifyMissing", "Spotify non rilevato", "Spotify not detected"),
        new("Detected", "rilevato", "detected"),
        new("NotDetected", "non rilevato", "not detected"),
        new("Missing", "mancante", "missing"),
        new("ImportOk", "Impostazioni importate correttamente.", "Settings imported successfully."),
        new("ExportOk", "Impostazioni esportate correttamente.", "Settings exported successfully."),
        new("NoUpdate", "Nessun aggiornamento macOS disponibile.", "No macOS update is available."),
        new("UpdateAvailable", "Nuovo aggiornamento macOS disponibile", "New macOS update available"),
        new("GuardNoSpotify", "Versione Spotify non determinabile.", "Spotify version could not be determined."),
        new("GuardFirstRun", "Versione Spotify registrata.", "Spotify version recorded."),
        new("GuardUnchanged", "Versione Spotify invariata.", "Spotify version unchanged."),
        new("GuardChanged", "Aggiornamento Spotify rilevato.", "Spotify update detected."),
        new("GuardRepairOk", "Riparazione Spicetify completata.", "Spicetify repair completed."),
        new("GuardRepairFailed", "Riparazione Spicetify non riuscita.", "Spicetify repair failed.")
    };

    public string Translate(string text, string language)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var entry = Entries.FirstOrDefault(
            x => string.Equals(x.It, text, StringComparison.Ordinal) ||
                 string.Equals(x.En, text, StringComparison.Ordinal));

        if (entry is null)
            return text;

        return language == "English" ? entry.En : entry.It;
    }

    public string Get(string key, string language)
    {
        var entry = Entries.FirstOrDefault(
            x => string.Equals(x.Key, key, StringComparison.Ordinal));

        if (entry is null)
            return key;

        return language == "English" ? entry.En : entry.It;
    }

    public void Apply(Control root, string language)
    {
        foreach (var node in root.GetSelfAndLogicalDescendants())
        {
            switch (node)
            {
                case TextBlock textBlock when !string.IsNullOrWhiteSpace(textBlock.Text):
                    textBlock.Text = Translate(textBlock.Text!, language);
                    break;

                case HeaderedContentControl headered when headered.Header is string header:
                    headered.Header = Translate(header, language);
                    break;

                case ContentControl contentControl when contentControl.Content is string content:
                    contentControl.Content = Translate(content, language);
                    break;
            }
        }
    }
}
