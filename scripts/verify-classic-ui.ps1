$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$mainPath = Join-Path $repo 'src\ToastifyReloaded\MainWindow.xaml'
$toastPath = Join-Path $repo 'src\ToastifyReloaded\ToastWindow.xaml'
$appPath = Join-Path $repo 'src\ToastifyReloaded\App.xaml'
$hotkeyPath = Join-Path $repo 'src\ToastifyReloaded\Services\GlobalHotkeyService.cs'

$main = Get-Content -Raw -LiteralPath $mainPath
$toast = Get-Content -Raw -LiteralPath $toastPath
$app = Get-Content -Raw -LiteralPath $appPath
$hotkeys = Get-Content -Raw -LiteralPath $hotkeyPath

function Require-Text([string]$Content, [string]$Needle, [string]$Description) {
    if (-not $Content.Contains($Needle)) {
        throw "Classic UI guard failed: $Description (`'$Needle`' not found)."
    }
}

Require-Text $main 'Height="570" Width="580" ResizeMode="NoResize"' 'settings window must remain 580x570 and fixed-size'
Require-Text $main '<TabItem Header="General"' 'historical General tab'
Require-Text $main '<TabItem Header="Hotkeys"' 'historical Hotkeys tab'
Require-Text $main '<TabItem Header="Toast"' 'historical Toast tab'
Require-Text $main '<TabItem Header="Advanced"' 'historical Advanced tab'
Require-Text $main '<TabItem Header="Reloaded"' 'single added Reloaded tab'
Require-Text $main 'Margin="0,32,90,0"' 'historical Save button position'
Require-Text $main 'Width="47"' 'historical Save button width'
Require-Text $main 'Margin="0,32,10,0"' 'historical Default split-button position'
Require-Text $main 'Width="73"' 'historical Default split-button width'
Require-Text $main 'Height="120" Width="120"' 'historical General-tab logo geometry'
Require-Text $main 'x:Name="FadeInUpDown"' 'Toast fade-in setting'
Require-Text $main 'x:Name="FadeOutUpDown"' 'Toast fade-out setting'
Require-Text $main 'VerticalScrollBarVisibility="Auto"' 'DPI-safe scrollable settings surfaces'
Require-Text $main 'Text="{}{0}"' 'escaped historical clipboard template'
if ($main.Contains('Text="{0}"')) { throw 'Classic UI guard failed: unescaped {0} XAML markup extension reintroduced.' }

$topTabs = [regex]::Matches($main, '<TabItem\s+Header="([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
$expected = @('General','Hotkeys','Toast','Advanced','Reloaded')
# Nested Toast tabs are also matched, so filter the known nested names before checking order.
$topTabs = $topTabs | Where-Object { $_ -notin @('Colors &amp; Font') }
$generalIndexes = @()
for ($i=0; $i -lt $topTabs.Count; $i++) { if ($topTabs[$i] -eq 'General') { $generalIndexes += $i } }
if ($generalIndexes.Count -gt 1) { $topTabs = $topTabs | Select-Object -Skip 0 }
# The first three and final two top-level markers are sufficient to detect accidental reorderings.
if ($topTabs[0] -ne 'General' -or $topTabs[1] -ne 'Hotkeys' -or $topTabs[2] -ne 'Toast' -or $topTabs[-2] -ne 'Advanced' -or $topTabs[-1] -ne 'Reloaded') {
    throw "Classic UI guard failed: top-level tab order changed. Found: $($topTabs -join ', ')"
}

Require-Text $toast 'Width="250" Height="70"' 'historical toast size'
Require-Text $toast 'BorderBrush="#FF292929"' 'historical toast border color'
Require-Text $toast 'BorderThickness="1" CornerRadius="4"' 'historical toast border geometry'
Require-Text $toast 'Color="#FF555555" Offset="0"' 'historical toast top gradient'
Require-Text $toast 'Color="#FF151515" Offset="1"' 'historical toast bottom gradient'
Require-Text $toast '<ColumnDefinition Width="70"' 'historical toast artwork column'
Require-Text $toast 'Height="60" Width="60"' 'historical toast artwork size'
Require-Text $toast 'Margin="15,15,0,4"' 'historical toast content margin'
Require-Text $toast 'FontSize="16"' 'historical first title size'
Require-Text $toast 'FontSize="12"' 'historical second title size'
Require-Text $toast 'Background="#FF333333"' 'historical progress background'
Require-Text $toast 'Background="#FFA0A0A0"' 'historical progress foreground'
Require-Text $hotkeys 'ToastifyReloaded.GlobalHotkeySink' 'dedicated global hotkey message sink'
Require-Text $hotkeys 'RegisterHotKey' 'system-wide hotkey registration'

if ($app -match '<Style\b') {
    throw 'Classic UI guard failed: App.xaml contains a global custom Style. Native WPF styles must remain untouched.'
}

$forbidden = @('Spotify Popup & Lyrics System','Tutti i sistemi operativi','Nascondi nella tray','Stato Spotify')
foreach ($text in $forbidden) {
    if ($main.Contains($text)) { throw "Classic UI guard failed: Reloaded dashboard text reintroduced: $text" }
}

Write-Host 'Classic Toastify 1.11.2 UI invariants: PASS'
