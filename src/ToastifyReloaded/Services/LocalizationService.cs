using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ToastifyReloaded.Services;

public static class LocalizationService
{
    public const string English = "English";
    public const string Italian = "Italian";

    public static string CurrentLanguage { get; private set; } = English;

    private static readonly IReadOnlyDictionary<string, string> EnToIt = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Toastify Reloaded - Settings"] = "Toastify Reloaded - Impostazioni",
        ["General"] = "Generale",
        ["English"] = "Inglese",
        ["Port"] = "Porta",
        ["Preview & behaviour"] = "Anteprima e comportamento",
        ["Hotkeys"] = "Scorciatoie",
        ["Toast"] = "Toast",
        ["Advanced"] = "Avanzate",
        ["Reloaded"] = "Reloaded",
        ["Colors & Font"] = "Colori e font",
        ["Themes"] = "Temi",
        ["Animations"] = "Animazioni",
        ["Position"] = "Posizione",
        ["Save"] = "Salva",
        ["Settings"] = "Impostazioni",
        ["Show Toast"] = "Mostra Toast",
        ["Exit"] = "Esci",
        ["Default"] = "Predefiniti",
        ["Default All"] = "Ripristina tutto",
        ["Reset this tab"] = "Ripristina questa scheda",
        ["Reset all settings"] = "Ripristina tutte le impostazioni",
        ["Reset the current tab's settings to their default values"] = "Ripristina ai valori predefiniti le impostazioni della scheda corrente",
        ["Appearance"] = "Aspetto",
        ["Application theme:"] = "Tema applicazione:",
        ["Interface language:"] = "Lingua interfaccia:",
        ["Light"] = "Chiaro",
        ["Dark"] = "Scuro",
        ["Follow Windows"] = "Segui Windows",
        ["Light preserves the classic Toastify appearance. Dark applies a native-style dark palette. Follow Windows tracks the current Windows app theme."] = "Chiaro mantiene l'aspetto classico di Toastify. Scuro applica una palette scura in stile nativo. Segui Windows usa il tema attuale delle app di Windows.",

        ["Launch Toastify with Windows"] = "Avvia Toastify con Windows",
        ["Minimize Spotify on launch"] = "Riduci Spotify all'avvio",
        ["Close Spotify when Toastify is closed"] = "Chiudi Spotify quando Toastify viene chiuso",
        ["Volume control mode:"] = "Modalità controllo volume:",
        ["Windows Volume Mixer"] = "Mixer volume di Windows",
        ["System media keys"] = "Tasti multimediali di sistema",
        ["Windows Volume Mixer increment:"] = "Incremento Mixer volume di Windows:",
        ["Clipboard & track export"] = "Appunti ed esportazione brano",
        ["Clipboard Template:"] = "Modello appunti:",
        ["{0} is replaced with the currently playing song when the Copy to Clipboard hotkey is used."] = "{0} viene sostituito con il brano attualmente in riproduzione quando viene usata la scorciatoia Copia negli appunti.",
        ["Save Artist and Album to file when song changes (uses the clipboard template)"] = "Salva Artista e Album su file quando cambia brano (usa il modello appunti)",
        ["Select File"] = "Seleziona file",
        ["Opt in to anonymous data collection. Data is never shared, is anonymous, and is used solely to improve the app."] = "Consenti la raccolta anonima di dati. I dati non vengono condivisi, sono anonimi e vengono usati solo per migliorare l'app.",
        ["Updates"] = "Aggiornamenti",
        ["Update check frequency:"] = "Frequenza controllo aggiornamenti:",
        ["Every time Toastify starts"] = "A ogni avvio di Toastify",
        ["Every 6 hours"] = "Ogni 6 ore",
        ["Daily"] = "Ogni giorno",
        ["Never"] = "Mai",
        ["Configure auto updates:"] = "Aggiornamenti automatici:",
        ["Download and install automatically"] = "Scarica e installa automaticamente",
        ["Notify only"] = "Solo notifica",

        ["Enable Global Hotkeys"] = "Abilita scorciatoie globali",
        ["Selected hotkey"] = "Scorciatoia selezionata",
        ["Windows Key"] = "Tasto Windows",
        ["Press a Key:"] = "Premi un tasto:",
        ["Global hotkeys are registered through a dedicated hidden native window, so they continue to work when Toastify is in the tray or another application is focused."] = "Le scorciatoie globali vengono registrate tramite una finestra nativa nascosta dedicata, quindi continuano a funzionare quando Toastify è nella tray o un'altra applicazione è in primo piano.",
        ["Play / Pause"] = "Riproduci / Pausa",
        ["Next Track"] = "Brano successivo",
        ["Previous Track"] = "Brano precedente",
        ["Volume Up"] = "Volume su",
        ["Volume Down"] = "Volume giù",
        ["Mute"] = "Muto",
        ["Seek Forward"] = "Avanti",
        ["Seek Backward"] = "Indietro",

        ["Disable Toast"] = "Disabilita Toast",
        ["Behaviour"] = "Comportamento",
        ["Only Show Toast when Hotkey is pressed"] = "Mostra il Toast solo quando viene premuta la scorciatoia",
        ["Disable Toast when a fullscreen app is running"] = "Disabilita il Toast quando è attiva un'app a schermo intero",
        ["Show song progress bar"] = "Mostra barra di avanzamento del brano",
        ["Show song time / duration"] = "Mostra tempo / durata brano",
        ["Display Time:"] = "Durata visualizzazione:",
        ["Artist and Track name:"] = "Artista e titolo brano:",
        ["Track name / Artist"] = "Titolo brano / Artista",
        ["Artist / Track name"] = "Artista / Titolo brano",
        ["Adaptive size & artwork"] = "Dimensione adattiva e copertina",
        ["Automatically resize Toast to Track / Artist"] = "Ridimensiona automaticamente il Toast in base a brano / artista",
        ["Min width:"] = "Larghezza min:",
        ["Max width:"] = "Larghezza max:",
        ["Toast image:"] = "Immagine Toast:",
        ["Album cover"] = "Copertina album",
        ["Toastify Reloaded icon"] = "Icona Toastify Reloaded",
        ["None"] = "Nessuna",
        ["Use Toastify icon when album cover is unavailable"] = "Usa l'icona Toastify quando la copertina non è disponibile",
        ["Geometry"] = "Geometria",
        ["Toast Width:"] = "Larghezza Toast:",
        ["Toast Height:"] = "Altezza Toast:",
        ["Border Thickness:"] = "Spessore bordo:",
        ["Border Corner Radius"] = "Raggio angoli bordo",
        ["The classic 250 × 70 geometry remains the minimum/default."] = "La geometria classica 250 × 70 rimane il minimo/predefinito.",

        ["Background & Border"] = "Sfondo e bordo",
        ["Top color:"] = "Colore superiore:",
        ["Bottom color:"] = "Colore inferiore:",
        ["Border color:"] = "Colore bordo:",
        ["Offset:"] = "Offset:",
        ["Song Progress Bar"] = "Barra avanzamento brano",
        ["Background color:"] = "Colore sfondo:",
        ["Foreground color:"] = "Colore primo piano:",
        ["Text"] = "Testo",
        ["Top title:"] = "Titolo superiore:",
        ["Bottom title:"] = "Titolo inferiore:",
        ["Font size:"] = "Dimensione font:",
        ["Drop Shadow"] = "Ombra",
        ["Depth:"] = "Profondità:",
        ["Blur:"] = "Sfocatura:",

        ["Theme preset:"] = "Preset tema:",
        ["Test Toast"] = "Test Toast",
        ["Track title"] = "Titolo brano",
        ["Artist name"] = "Nome artista",
        ["The original Toastify gray-to-black look."] = "L'aspetto originale di Toastify, dal grigio al nero.",
        ["Current manual Colors & Font configuration."] = "Configurazione manuale corrente di Colori e font.",
        ["13 built-in themes are included. Manual changes in Colors & Font remain available and are stored as Custom when no preset matches."] = "Sono inclusi 13 temi. Le modifiche manuali in Colori e font restano disponibili e vengono salvate come Personalizzato quando nessun preset corrisponde.",
        ["Presets change colors, border geometry and optional text shadows only. Size, album artwork, position and animation settings remain independent."] = "I preset modificano solo colori, geometria del bordo e ombre del testo opzionali. Dimensione, copertina album, posizione e animazioni restano indipendenti.",
        ["Custom"] = "Personalizzato",

        ["Animation"] = "Animazione",
        ["Animation style:"] = "Stile animazione:",
        ["Fade"] = "Dissolvenza",
        ["Slide"] = "Scorrimento",
        ["Fade + Slide"] = "Dissolvenza + Scorrimento",
        ["Slide in direction:"] = "Direzione entrata:",
        ["Slide out direction:"] = "Direzione uscita:",
        ["Up"] = "Su",
        ["Down"] = "Giù",
        ["Left"] = "Sinistra",
        ["Right"] = "Destra",
        ["Fade / enter time:"] = "Tempo dissolvenza / entrata:",
        ["Fade / exit time:"] = "Tempo dissolvenza / uscita:",
        ["Slide in distance:"] = "Distanza entrata:",
        ["Slide out distance:"] = "Distanza uscita:",
        ["Fade keeps the classic dissolve. Slide moves the toast in and out. Fade + Slide combines both effects. None shows and hides immediately."] = "Dissolvenza mantiene l'effetto classico. Scorrimento muove il Toast in entrata e uscita. Dissolvenza + Scorrimento combina i due effetti. Nessuna mostra e nasconde immediatamente.",
        ["Slide In and Slide Out can use different directions and distances. Fade In/Fade Out keep independent timing. Display Time is still the fully-visible duration between the two animations."] = "Entrata e uscita possono usare direzioni e distanze diverse. Fade In/Fade Out mantengono tempi indipendenti. La durata visualizzazione resta il tempo in cui il Toast è completamente visibile tra le due animazioni.",

        ["Screen placement"] = "Posizionamento schermo",
        ["Position preset:"] = "Preset posizione:",
        ["Monitor:"] = "Monitor:",
        ["Screen margin:"] = "Margine schermo:",
        ["Primary monitor"] = "Monitor principale",
        ["Primary"] = "Principale",
        ["Top Left"] = "In alto a sinistra",
        ["Top left"] = "In alto a sinistra",
        ["Top center"] = "In alto al centro",
        ["Top Right"] = "In alto a destra",
        ["Top right"] = "In alto a destra",
        ["Middle left"] = "Centro sinistra",
        ["Center"] = "Centro",
        ["Middle right"] = "Centro destra",
        ["Bottom left"] = "In basso a sinistra",
        ["Bottom Left"] = "In basso a sinistra",
        ["Bottom center"] = "In basso al centro",
        ["Bottom right"] = "In basso a destra",
        ["Bottom Right"] = "In basso a destra",
        ["Custom X:"] = "X personalizzata:",
        ["Custom Y:"] = "Y personalizzata:",
        ["Multi-monitor"] = "Multi-monitor",
        ["Choose Primary monitor or a specific connected display. Position presets are calculated against that monitor's working area and taskbar."] = "Scegli il monitor principale o uno schermo collegato specifico. I preset di posizione vengono calcolati sull'area di lavoro e sulla barra delle applicazioni di quel monitor.",

        ["Advanced integrations"] = "Integrazioni avanzate",
        ["Enable Spotify WebAPI"] = "Abilita Spotify WebAPI",
        ["Enable Broadcaster"] = "Abilita Broadcaster",
        ["WebSocket Port:"] = "Porta WebSocket:",
        ["Import / export settings"] = "Importazione / esportazione impostazioni",
        ["Back up the full Toastify Reloaded configuration, including toast appearance, global hotkeys, maintenance options and UI preferences."] = "Esegui il backup dell'intera configurazione di Toastify Reloaded, inclusi aspetto del Toast, scorciatoie globali, opzioni di manutenzione e preferenze dell'interfaccia.",
        ["Export settings"] = "Esporta impostazioni",
        ["Import settings"] = "Importa impostazioni",
        ["Diagnostics & compatibility reporting"] = "Diagnostica e compatibilità",
        ["Create a support report containing Toastify Reloaded, Windows, architecture, Spotify, Spicetify and Compatibility Guard status. The report does not include Spotify credentials or song history."] = "Crea un rapporto di supporto con stato di Toastify Reloaded, Windows, architettura, Spotify, Spicetify e Compatibility Guard. Il rapporto non include credenziali Spotify né cronologia dei brani.",
        ["Copy report"] = "Copia rapporto",
        ["Export report"] = "Esporta rapporto",
        ["Additional tools"] = "Strumenti aggiuntivi",
        ["Run diagnostics helper"] = "Avvia strumento diagnostica",
        ["Open configuration folder"] = "Apri cartella configurazione",
        ["The historical current-tab Default action remains available from the top-right split button for selective reset."] = "L'azione Predefiniti della scheda corrente resta disponibile dal pulsante diviso in alto a destra per il ripristino selettivo.",

        ["Lyrics Plus"] = "Lyrics Plus",
        ["Spicetify integration used to keep the Lyrics button available inside Spotify."] = "Integrazione Spicetify usata per mantenere disponibile il pulsante Lyrics dentro Spotify.",
        ["Install / enable"] = "Installa / abilita",
        ["Repair after Spotify update"] = "Ripara dopo aggiornamento Spotify",
        ["Remove"] = "Rimuovi",
        ["Tools"] = "Strumenti",
        ["Run diagnostics"] = "Esegui diagnostica",
        ["Updates & Compatibility Guard"] = "Aggiornamenti e Compatibility Guard",
        ["Toastify Reloaded version:"] = "Versione Toastify Reloaded:",
        ["Spotify version:"] = "Versione Spotify:",
        ["Spicetify version:"] = "Versione Spicetify:",
        ["Compatibility status:"] = "Stato compatibilità:",
        ["Checking…"] = "Controllo…",
        ["Automatically check Toastify Reloaded updates"] = "Controlla automaticamente gli aggiornamenti di Toastify Reloaded",
        ["Automatically install Toastify Reloaded updates"] = "Installa automaticamente gli aggiornamenti di Toastify Reloaded",
        ["Repair Spicetify automatically when Spotify changes version"] = "Ripara automaticamente Spicetify quando cambia la versione di Spotify",
        ["Keep Lyrics Plus enabled"] = "Mantieni Lyrics Plus abilitato",
        ["Upgrade Spicetify before repair"] = "Aggiorna Spicetify prima della riparazione",
        ["Restart Spotify after repair"] = "Riavvia Spotify dopo la riparazione",
        ["Updates have not been checked yet."] = "Gli aggiornamenti non sono ancora stati controllati.",
        ["Check updates"] = "Controlla aggiornamenti",
        ["Install update"] = "Installa aggiornamento",
        ["Check compatibility"] = "Controlla compatibilità",
        ["Repair now"] = "Ripara ora"
    };

    private static readonly IReadOnlyDictionary<string, string> ItToEn =
        EnToIt
            .GroupBy(pair => pair.Value, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Key, StringComparer.OrdinalIgnoreCase);

    public static string Get(string? language, string key, string english) =>
        TranslateText(language, english);

    public static string TranslateText(string? language, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text ?? string.Empty;

        var english = ItToEn.TryGetValue(text, out var normalizedEnglish) ? normalizedEnglish : text;
        if (string.Equals(language, Italian, StringComparison.OrdinalIgnoreCase) &&
            EnToIt.TryGetValue(english, out var italian))
            return italian;

        return english;
    }

    public static void ApplyToTree(DependencyObject root, string? language)
    {
        CurrentLanguage = string.Equals(language, Italian, StringComparison.OrdinalIgnoreCase) ? Italian : English;
        var visited = new HashSet<DependencyObject>();

        void Walk(object? value)
        {
            if (value is not DependencyObject node || !visited.Add(node))
                return;

            switch (node)
            {
                case TextBlock textBlock when !string.IsNullOrWhiteSpace(textBlock.Text):
                    textBlock.Text = TranslateText(CurrentLanguage, textBlock.Text);
                    break;
                case HeaderedContentControl headeredContent when headeredContent.Header is string header:
                    headeredContent.Header = TranslateText(CurrentLanguage, header);
                    break;
                case HeaderedItemsControl headeredItems when headeredItems.Header is string header:
                    headeredItems.Header = TranslateText(CurrentLanguage, header);
                    break;
                case ContentControl contentControl when contentControl.Content is string content:
                    contentControl.Content = TranslateText(CurrentLanguage, content);
                    break;
            }

            if (node is FrameworkElement frameworkElement && frameworkElement.ToolTip is string tooltip)
                frameworkElement.ToolTip = TranslateText(CurrentLanguage, tooltip);

            foreach (var child in LogicalTreeHelper.GetChildren(node))
                Walk(child);
        }

        Walk(root);
    }
}

public sealed class LocalizedTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        LocalizationService.TranslateText(LocalizationService.CurrentLanguage, value?.ToString());

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Binding.DoNothing;
}
