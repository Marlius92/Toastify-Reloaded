$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$mainPath = Join-Path $repo 'src\ToastifyReloaded\MainWindow.xaml'
$toastPath = Join-Path $repo 'src\ToastifyReloaded\ToastWindow.xaml'
$appPath = Join-Path $repo 'src\ToastifyReloaded\App.xaml'
$hotkeyPath = Join-Path $repo 'src\ToastifyReloaded\Services\GlobalHotkeyService.cs'
$themeServicePath = Join-Path $repo 'src\ToastifyReloaded\Services\ApplicationThemeService.cs'
$presetPath = Join-Path $repo 'src\ToastifyReloaded\Models\ToastThemePreset.cs'

$main = Get-Content -Raw -LiteralPath $mainPath
$toast = Get-Content -Raw -LiteralPath $toastPath
$app = Get-Content -Raw -LiteralPath $appPath
$hotkeys = Get-Content -Raw -LiteralPath $hotkeyPath
$themeService = Get-Content -Raw -LiteralPath $themeServicePath
$presets = Get-Content -Raw -LiteralPath $presetPath

function Require-Text([string]$Content, [string]$Needle, [string]$Description) {
    if (-not $Content.Contains($Needle)) {
        throw "UI contract failed: $Description (`'$Needle`' not found)."
    }
}

# v1.3.0 deliberately expands the historical shell to avoid visible scrollbars.
Require-Text $main 'Height="700" Width="840" ResizeMode="NoResize"' 'expanded fixed-size settings window'
Require-Text $main '<TabItem Header="General" x:Name="General"' 'historical General tab'
Require-Text $main '<TabItem Header="Hotkeys" x:Name="Hotkeys"' 'historical Hotkeys tab'
Require-Text $main '<TabItem Header="Toast" x:Name="TabToast"' 'historical Toast tab'
Require-Text $main '<TabItem Header="Advanced" x:Name="TabAdvanced"' 'historical Advanced tab'
Require-Text $main '<TabItem Header="Reloaded" x:Name="TabReloaded"' 'Reloaded extension tab'
Require-Text $main 'x:Name="BtnSave"' 'Save button'
Require-Text $main 'x:Name="BtnDefault"' 'Default split button'
Require-Text $main 'Text="{}{0}"' 'escaped historical clipboard template'

# One ScrollViewer is intentionally used inside the Dark-mode ComboBox popup template.
# User-facing settings pages themselves must not depend on scrollable surfaces.
$scrollViewerCount = ([regex]::Matches($main, '<ScrollViewer\b')).Count
if ($scrollViewerCount -gt 1) {
    throw "UI contract failed: unexpected user-facing ScrollViewer surfaces detected ($scrollViewerCount)."
}
if ($main -match '(?i)proxy') {
    throw 'UI contract failed: obsolete Proxy controls were reintroduced.'
}

if ($main.Contains('ApplyToastThemeButton') -or $main.Contains('Apply preset')) {
    throw 'UI contract failed: obsolete Apply preset workflow was reintroduced.'
}

# New v1.3.0 user-facing roadmap features.
Require-Text $main 'x:Name="ApplicationThemeComboBox"' 'application Light/Dark/System theme selector'
Require-Text $main 'x:Name="ApplicationLanguageComboBox"' 'localization selector'
Require-Text $main 'x:Name="ToastThemesTab"' 'Toast theme presets tab'
Require-Text $main 'x:Name="ToastAnimationsTab"' 'Toast animation tab'
Require-Text $main 'x:Name="ToastPositionTab"' 'Toast positioning tab'
Require-Text $main 'x:Name="ToastThemePresetComboBox"' 'Toast theme preset selector'
Require-Text $main 'x:Name="ToastAnimationStyleComboBox"' 'Toast animation style selector'
Require-Text $main 'x:Name="ToastSlideInDirectionComboBox"' 'independent Slide In direction selector'
Require-Text $main 'x:Name="ToastSlideOutDirectionComboBox"' 'independent Slide Out direction selector'
Require-Text $main 'x:Name="ToastSlideInDistanceUpDown"' 'independent Slide In distance'
Require-Text $main 'x:Name="ToastSlideOutDistanceUpDown"' 'independent Slide Out distance'
Require-Text $main 'x:Name="CbShowSongDuration"' 'optional song time/duration selector'
Require-Text $main 'x:Name="ToastMonitorComboBox"' 'multi-monitor selector'
Require-Text $main 'Click="ExportSettings_Click"' 'settings export action'
Require-Text $main 'Click="ImportSettings_Click"' 'settings import action'
Require-Text $main 'Click="CopyDiagnostics_Click"' 'diagnostic report copy action'
Require-Text $main 'Click="ExportDiagnostics_Click"' 'diagnostic report export action'

# The actual popup preserves the classic minimum geometry and visual lineage.
Require-Text $toast 'Width="250" Height="70"' 'historical toast default size'
Require-Text $toast 'BorderBrush="#FF292929"' 'historical toast border color'
Require-Text $toast 'BorderThickness="1" CornerRadius="4"' 'historical toast border geometry'
Require-Text $toast 'Color="#FF555555" Offset="0"' 'historical toast top gradient'
Require-Text $toast 'Color="#FF151515" Offset="1"' 'historical toast bottom gradient'
Require-Text $toast 'x:Name="ArtworkColumn" Width="70"' 'historical artwork column'
Require-Text $toast 'Height="60" Width="60"' 'historical artwork geometry'
Require-Text $toast 'FontSize="16"' 'historical first title size'
Require-Text $toast 'FontSize="12"' 'historical second title size'
Require-Text $toast 'x:Name="ToastTranslate"' 'slide animation transform'
Require-Text $toast 'x:Name="SongDurationText"' 'optional current / total song time'
Require-Text $toast 'x:Name="SongTimelineGrid"' 'shared song timeline row'

Require-Text $hotkeys 'ToastifyReloaded.GlobalHotkeySink' 'dedicated global hotkey message sink'
Require-Text $hotkeys 'RegisterHotKey' 'system-wide hotkey registration'
Require-Text $themeService 'AppsUseLightTheme' 'Follow Windows theme detection'
Require-Text $themeService 'DwmSetWindowAttribute' 'Windows title-bar dark mode support'

$requiredPresets = @(
    'Classic Toastify','Spotify Green','Midnight Blue','Neon Purple','Cyberpunk',
    'Crimson Night','Amber Gold','Emerald','Ocean','Sakura','Arctic','Monochrome','Retro Synthwave'
)
foreach ($preset in $requiredPresets) {
    Require-Text $presets ('"' + $preset + '"') "Toast theme preset $preset"
}

if ($app -match '<Style\b') {
    throw 'UI contract failed: App.xaml contains a global custom Style. Theme styling must remain local to the settings window.'
}

$forbidden = @('Spotify Popup & Lyrics System','Tutti i sistemi operativi','Nascondi nella tray','Stato Spotify')
foreach ($text in $forbidden) {
    if ($main.Contains($text)) { throw "UI contract failed: old Reloaded dashboard text reintroduced: $text" }
}

Write-Host 'Toastify Reloaded v1.3.2 UI + roadmap contract: PASS'
