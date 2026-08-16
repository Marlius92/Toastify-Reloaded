using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using ToastifyReloaded.Models;
using ToastifyReloaded.Native;
using ToastifyReloaded.Services;
using Forms = System.Windows.Forms;

namespace ToastifyReloaded;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService = new();
    private readonly SpotifySessionService _spotify = new();
    private readonly SpotifyInstallationService _spotifyInstallation = new();
    private readonly CompatibilityRepairService _compatibility = new();
    private readonly UpdateService _updateService = new();
    private readonly DispatcherTimer _pollTimer;
    private readonly DispatcherTimer _compatibilityTimer;
    private readonly DispatcherTimer _updateTimer;
    private readonly ObservableCollection<HotkeyBinding> _hotkeys = new();

    private AppSettings _settings = new();
    private GlobalHotkeyService? _globalHotkeys;
    private Forms.NotifyIcon? _trayIcon;
    private HotkeyBinding? _selectedHotkey;
    private string? _lastTrackIdentity;
    private bool _reallyExit;
    private bool _maintenanceRunning;
    private bool _updateCheckRunning;
    private bool _loadingHotkeyEditor;

    public MainWindow()
    {
        InitializeComponent();

        var warningIcon = LoadSystemIcon(32515); // IDI_WARNING
        AdvancedWarningImage.Source = warningIcon;
        HotkeyWarningImage.Source = warningIcon;
        ToastInfoImage.Source = LoadSystemIcon(32516); // IDI_INFORMATION

        _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1400) };
        _pollTimer.Tick += async (_, _) => await PollSpotifyAsync();

        _compatibilityTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
        _compatibilityTimer.Tick += async (_, _) => await RunCompatibilityCheckAsync(automatic: true);

        _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromHours(6) };
        _updateTimer.Tick += async (_, _) => await CheckForToastifyUpdatesAsync(allowAutomaticInstall: true);

        Loaded += MainWindow_Loaded;
        SourceInitialized += MainWindow_SourceInitialized;
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _settings = _settingsService.Load();
        _settings.StartWithWindows = StartupService.IsEnabled();

        _hotkeys.Clear();
        foreach (var item in _settings.Hotkeys)
            _hotkeys.Add(item);
        LstHotKeys.ItemsSource = _hotkeys;
        if (_hotkeys.Count > 0)
            LstHotKeys.SelectedIndex = 0;

        LoadSettingsIntoUi();
        LoadMaintenanceSettingsIntoUi();
        AppVersionText.Text = _updateService.CurrentVersion;

        CreateTrayIcon();
        RegisterHotkeys(showSuccess: false);
        await RefreshSpotifyStatusAsync();
        _pollTimer.Start();
        _compatibilityTimer.Start();
        _updateTimer.Start();

        if (_settings.MinimizeSpotifyOnStartup)
            TryMinimizeSpotify();

        if (_settings.AutoCheckToastifyUpdates)
        {
            var updateStarted = await CheckForToastifyUpdatesAsync(allowAutomaticInstall: true);
            if (updateStarted)
                return;
        }

        await RunCompatibilityCheckAsync(automatic: true);

        if (_settings.StartMinimized || Environment.GetCommandLineArgs().Any(a => a.Equals("--minimized", StringComparison.OrdinalIgnoreCase)))
            Hide();
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;
        _globalHotkeys = new GlobalHotkeyService(handle);
        _globalHotkeys.HotkeyPressed += async (_, action) => await ExecuteActionAsync(action);
    }

    private void CreateTrayIcon()
    {
        _trayIcon = new Forms.NotifyIcon
        {
            Text = "Toastify Reloaded",
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!) ?? System.Drawing.SystemIcons.Application,
            Visible = true
        };

        // Keep the tray surface intentionally simple, like classic Toastify.
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Settings", null, (_, _) => ShowFromTray());
        menu.Items.Add("Show Toast", null, async (_, _) => await ShowCurrentToastAsync());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApplication());
        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => ShowFromTray();
    }

    private void LoadSettingsIntoUi()
    {
        StartWithWindowsCheckBox.IsChecked = _settings.StartWithWindows;
        MinimizeSpotifyCheckBox.IsChecked = _settings.MinimizeSpotifyOnStartup;
        CloseSpotifyCheckBox.IsChecked = _settings.CloseSpotifyWithToastify;
        ComboVolumeControlMode.SelectedIndex = _settings.VolumeControlMode.Equals("System media keys", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        WindowsMixerIncrementUpDown.Value = _settings.WindowsVolumeMixerIncrement;
        TbClipboardTemplate.Text = string.IsNullOrWhiteSpace(_settings.ClipboardTemplate) ? "{0}" : _settings.ClipboardTemplate;
        CbSaveTrackToFile.IsChecked = _settings.SaveTrackToFile;
        CbAnalytics.IsChecked = _settings.OptInToAnalytics;
        CbHotkeys.IsChecked = _settings.GlobalHotkeysEnabled;

        CbDisableToast.IsChecked = !_settings.ShowToastOnTrackChange;
        CbOnlyShowToastOnHotkey.IsChecked = _settings.OnlyShowToastOnHotkey;
        CbDisableToastFullscreen.IsChecked = _settings.DisableToastWithFullscreenApps;
        CbShowSongProgressBar.IsChecked = _settings.ShowSongProgressBar;
        DisplayTimeUpDown.Value = _settings.ToastDurationMs;
        CbToastTitlesOrder.SelectedIndex = _settings.ToastTitlesOrder.Equals("ArtistTrack", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        ToastWidthUpDown.Value = _settings.ToastWidth;
        ToastHeightUpDown.Value = _settings.ToastHeight;
        PositionLeftUpDown.Value = _settings.PositionLeft;
        PositionTopUpDown.Value = _settings.PositionTop;
        BorderThicknessUpDown.Value = _settings.ToastBorderThickness;
        BorderTopLeftUpDown.Value = _settings.ToastCornerTopLeft;
        BorderTopRightUpDown.Value = _settings.ToastCornerTopRight;
        BorderBottomLeftUpDown.Value = _settings.ToastCornerBottomLeft;
        BorderBottomRightUpDown.Value = _settings.ToastCornerBottomRight;
        ToastColorTopPicker.SelectedColor = ParseColor(_settings.ToastColorTop, Color.FromArgb(255, 85, 85, 85));
        ToastColorBottomPicker.SelectedColor = ParseColor(_settings.ToastColorBottom, Color.FromArgb(255, 21, 21, 21));
        ToastBorderColorPicker.SelectedColor = ParseColor(_settings.ToastBorderColor, Color.FromArgb(255, 41, 41, 41));
        Title1ColorPicker.SelectedColor = ParseColor(_settings.ToastTitle1Color, Colors.White);
        Title2ColorPicker.SelectedColor = ParseColor(_settings.ToastTitle2Color, Color.FromArgb(255, 240, 240, 240));
        ColorTopUpDown.Value = _settings.ToastColorTopOffset;
        ColorBottomUpDown.Value = _settings.ToastColorBottomOffset;
        Title1FontSizeUpDown.Value = _settings.ToastTitle1FontSize;
        Title2FontSizeUpDown.Value = _settings.ToastTitle2FontSize;
        CbToastTitle1DropShadow.IsChecked = _settings.ToastTitle1DropShadow;
        CbToastTitle2DropShadow.IsChecked = _settings.ToastTitle2DropShadow;
        Title1ShadowDepthUpDown.Value = _settings.ToastTitle1ShadowDepth;
        Title1ShadowBlurUpDown.Value = _settings.ToastTitle1ShadowBlur;
        Title2ShadowDepthUpDown.Value = _settings.ToastTitle2ShadowDepth;
        Title2ShadowBlurUpDown.Value = _settings.ToastTitle2ShadowBlur;
        SongProgressBackgroundColorPicker.SelectedColor = ParseColor(_settings.SongProgressBarBackgroundColor, Color.FromArgb(255, 51, 51, 51));
        SongProgressForegroundColorPicker.SelectedColor = ParseColor(_settings.SongProgressBarForegroundColor, Color.FromArgb(255, 160, 160, 160));
        UpdateToastOptionsEnabledState();

        CbUseProxy.IsChecked = _settings.UseProxy;
        ProxyHostTextBox.Text = _settings.ProxyHost;
        ProxyPortTextBox.Text = _settings.ProxyPort;
        ProxyUsernameTextBox.Text = _settings.ProxyUsername;
        CbBypassProxyLocal.IsChecked = _settings.BypassProxyOnLocal;
        CbEnableSpotifyWebApi.IsChecked = _settings.EnableSpotifyWebApi;
        CbEnableBroadcaster.IsChecked = _settings.EnableBroadcaster;

        CbUpdateCheckFrequency.SelectedIndex = _settings.AutoCheckToastifyUpdates ? 1 : 3;
        CbConfigureAutoUpdates.SelectedIndex = _settings.AutoInstallToastifyUpdates ? 1 : 0;
    }

    private static Color ParseColor(string value, Color fallback)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(value)!;
        }
        catch
        {
            return fallback;
        }
    }

    private static string ColorToString(Color? color, string fallback) => color?.ToString() ?? fallback;

    private async Task PollSpotifyAsync()
    {
        var track = await _spotify.GetCurrentTrackAsync();
        if (track is null)
        {
            CurrentTrackText.Text = "No Spotify track detected";
            return;
        }

        CurrentTrackText.Text = $"{track.Artist} — {track.Title}";

        if (_lastTrackIdentity is null)
        {
            _lastTrackIdentity = track.Identity;
            return;
        }

        if (_lastTrackIdentity == track.Identity)
            return;

        _lastTrackIdentity = track.Identity;
        await SaveTrackIfRequestedAsync(track);

        if (_settings.ShowToastOnTrackChange && !_settings.OnlyShowToastOnHotkey)
            ShowToast(track);
    }

    private async Task SaveTrackIfRequestedAsync(TrackInfo track)
    {
        if (!_settings.SaveTrackToFile || string.IsNullOrWhiteSpace(_settings.TrackFilePath))
            return;

        try
        {
            var value = FormatTrackTemplate(track);
            await File.WriteAllTextAsync(_settings.TrackFilePath, value);
        }
        catch
        {
            // Track-file export must never interrupt playback polling.
        }
    }

    private string FormatTrackTemplate(TrackInfo track)
    {
        var song = $"{track.Artist} - {track.Title}";
        try
        {
            return string.Format(_settings.ClipboardTemplate ?? "{0}", song);
        }
        catch
        {
            return song;
        }
    }

    private async Task ExecuteActionAsync(HotkeyAction action)
    {
        try
        {
            switch (action)
            {
                case HotkeyAction.PlayPause:
                    await _spotify.PlayPauseAsync();
                    break;
                case HotkeyAction.NextTrack:
                    await _spotify.NextAsync();
                    break;
                case HotkeyAction.PreviousTrack:
                    await _spotify.PreviousAsync();
                    break;
                case HotkeyAction.VolumeUp:
                    MediaKeyService.VolumeUp();
                    break;
                case HotkeyAction.VolumeDown:
                    MediaKeyService.VolumeDown();
                    break;
                case HotkeyAction.Mute:
                    MediaKeyService.ToggleMute();
                    break;
                case HotkeyAction.SeekForward:
                    await _spotify.SeekRelativeAsync(TimeSpan.FromSeconds(10));
                    break;
                case HotkeyAction.SeekBackward:
                    await _spotify.SeekRelativeAsync(TimeSpan.FromSeconds(-10));
                    break;
                case HotkeyAction.ShowToast:
                    await ShowCurrentToastAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            SpotifyStatusText.Text = $"Command error: {ex.Message}";
        }
    }

    private void RegisterHotkeys(bool showSuccess)
    {
        if (_globalHotkeys is null)
            return;

        if (!_settings.GlobalHotkeysEnabled)
        {
            _globalHotkeys.RegisterAll(Array.Empty<HotkeyBinding>());
            return;
        }

        var errors = _globalHotkeys.RegisterAll(_hotkeys);
        if (errors.Count > 0)
        {
            MessageBox.Show(string.Join(Environment.NewLine, errors), "Some hotkeys could not be registered", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        else if (showSuccess)
        {
            MessageBox.Show("Settings saved.", "Toastify Reloaded", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async Task RefreshSpotifyStatusAsync()
    {
        try
        {
            await _spotify.InitializeAsync();
            SpotifyStatusText.Text = await _spotify.GetConnectionDescriptionAsync();
            await PollSpotifyAsync();
        }
        catch (Exception ex)
        {
            SpotifyStatusText.Text = $"Spotify unavailable: {ex.Message}";
        }
    }

    private async Task ShowCurrentToastAsync()
    {
        var track = await _spotify.GetCurrentTrackAsync() ?? TrackInfo.Empty;
        ShowToast(track);
    }

    private void ShowToast(TrackInfo track)
    {
        var toast = new ToastWindow(track, _settings);
        toast.ShowTimed();
    }

    private void SaveSettingsFromUi()
    {
        ApplyHotkeyEditorToSelected();

        _settings.StartWithWindows = StartWithWindowsCheckBox.IsChecked == true;
        _settings.MinimizeSpotifyOnStartup = MinimizeSpotifyCheckBox.IsChecked == true;
        _settings.CloseSpotifyWithToastify = CloseSpotifyCheckBox.IsChecked == true;
        _settings.VolumeControlMode = ComboVolumeControlMode.SelectedIndex == 1 ? "System media keys" : "Windows Volume Mixer";
        _settings.WindowsVolumeMixerIncrement = WindowsMixerIncrementUpDown.Value ?? 1.0;
        _settings.ClipboardTemplate = string.IsNullOrWhiteSpace(TbClipboardTemplate.Text) ? "{0}" : TbClipboardTemplate.Text;
        _settings.SaveTrackToFile = CbSaveTrackToFile.IsChecked == true;
        _settings.OptInToAnalytics = CbAnalytics.IsChecked == true;
        _settings.GlobalHotkeysEnabled = CbHotkeys.IsChecked == true;

        _settings.ShowToastOnTrackChange = CbDisableToast.IsChecked != true;
        _settings.OnlyShowToastOnHotkey = CbOnlyShowToastOnHotkey.IsChecked == true;
        _settings.DisableToastWithFullscreenApps = CbDisableToastFullscreen.IsChecked == true;
        _settings.ShowSongProgressBar = CbShowSongProgressBar.IsChecked == true;
        _settings.ToastDurationMs = Math.Clamp(DisplayTimeUpDown.Value ?? 3500, 500, 30000);
        _settings.ToastTitlesOrder = CbToastTitlesOrder.SelectedIndex == 1 ? "ArtistTrack" : "TrackArtist";
        _settings.ToastWidth = ToastWidthUpDown.Value ?? 250;
        _settings.ToastHeight = ToastHeightUpDown.Value ?? 70;
        _settings.PositionLeft = PositionLeftUpDown.Value ?? -1;
        _settings.PositionTop = PositionTopUpDown.Value ?? -1;
        _settings.ToastBorderThickness = BorderThicknessUpDown.Value ?? 1;
        _settings.ToastCornerTopLeft = BorderTopLeftUpDown.Value ?? 4;
        _settings.ToastCornerTopRight = BorderTopRightUpDown.Value ?? 4;
        _settings.ToastCornerBottomLeft = BorderBottomLeftUpDown.Value ?? 4;
        _settings.ToastCornerBottomRight = BorderBottomRightUpDown.Value ?? 4;
        _settings.ToastColorTop = ColorToString(ToastColorTopPicker.SelectedColor, "#FF555555");
        _settings.ToastColorBottom = ColorToString(ToastColorBottomPicker.SelectedColor, "#FF151515");
        _settings.ToastColorTopOffset = ColorTopUpDown.Value ?? 0;
        _settings.ToastColorBottomOffset = ColorBottomUpDown.Value ?? 1;
        _settings.ToastBorderColor = ColorToString(ToastBorderColorPicker.SelectedColor, "#FF292929");
        _settings.ToastTitle1Color = ColorToString(Title1ColorPicker.SelectedColor, "#FFFFFFFF");
        _settings.ToastTitle2Color = ColorToString(Title2ColorPicker.SelectedColor, "#FFF0F0F0");
        _settings.ToastTitle1FontSize = Title1FontSizeUpDown.Value ?? 16;
        _settings.ToastTitle2FontSize = Title2FontSizeUpDown.Value ?? 12;
        _settings.ToastTitle1DropShadow = CbToastTitle1DropShadow.IsChecked == true;
        _settings.ToastTitle2DropShadow = CbToastTitle2DropShadow.IsChecked == true;
        _settings.ToastTitle1ShadowDepth = Title1ShadowDepthUpDown.Value ?? 3;
        _settings.ToastTitle1ShadowBlur = Title1ShadowBlurUpDown.Value ?? 2;
        _settings.ToastTitle2ShadowDepth = Title2ShadowDepthUpDown.Value ?? 3;
        _settings.ToastTitle2ShadowBlur = Title2ShadowBlurUpDown.Value ?? 2;
        _settings.SongProgressBarBackgroundColor = ColorToString(SongProgressBackgroundColorPicker.SelectedColor, "#FF333333");
        _settings.SongProgressBarForegroundColor = ColorToString(SongProgressForegroundColorPicker.SelectedColor, "#FFA0A0A0");

        _settings.UseProxy = CbUseProxy.IsChecked == true;
        _settings.ProxyHost = ProxyHostTextBox.Text;
        _settings.ProxyPort = ProxyPortTextBox.Text;
        _settings.ProxyUsername = ProxyUsernameTextBox.Text;
        _settings.BypassProxyOnLocal = CbBypassProxyLocal.IsChecked == true;
        _settings.EnableSpotifyWebApi = CbEnableSpotifyWebApi.IsChecked == true;
        _settings.EnableBroadcaster = CbEnableBroadcaster.IsChecked == true;

        _settings.AutoCheckToastifyUpdates = CbUpdateCheckFrequency.SelectedIndex != 3 && AutoCheckUpdatesCheckBox.IsChecked == true;
        _settings.AutoInstallToastifyUpdates = CbConfigureAutoUpdates.SelectedIndex == 1 && AutoInstallUpdatesCheckBox.IsChecked == true;
        _settings.Hotkeys = _hotkeys.ToList();
        SaveMaintenanceSettingsFromUi();

        StartupService.SetEnabled(_settings.StartWithWindows);
        _settingsService.Save(_settings);
    }

    private void LoadMaintenanceSettingsIntoUi()
    {
        AutoCheckUpdatesCheckBox.IsChecked = _settings.AutoCheckToastifyUpdates;
        AutoInstallUpdatesCheckBox.IsChecked = _settings.AutoInstallToastifyUpdates;
        AutoRepairSpotifyCheckBox.IsChecked = _settings.AutoRepairAfterSpotifyUpdate;
        KeepLyricsCheckBox.IsChecked = _settings.KeepLyricsPlusEnabled;
        AutoUpgradeSpicetifyCheckBox.IsChecked = _settings.AutoUpgradeSpicetify;
        RestartSpotifyCheckBox.IsChecked = _settings.RestartSpotifyAfterRepair;
    }

    private void SaveMaintenanceSettingsFromUi()
    {
        _settings.AutoCheckToastifyUpdates = AutoCheckUpdatesCheckBox.IsChecked == true;
        _settings.AutoInstallToastifyUpdates = AutoInstallUpdatesCheckBox.IsChecked == true;
        _settings.AutoRepairAfterSpotifyUpdate = AutoRepairSpotifyCheckBox.IsChecked == true;
        _settings.KeepLyricsPlusEnabled = KeepLyricsCheckBox.IsChecked == true;
        _settings.AutoUpgradeSpicetify = AutoUpgradeSpicetifyCheckBox.IsChecked == true;
        _settings.RestartSpotifyAfterRepair = RestartSpotifyCheckBox.IsChecked == true;
    }

    private async Task<bool> CheckForToastifyUpdatesAsync(bool allowAutomaticInstall, bool forceInstall = false)
    {
        if (_updateCheckRunning)
            return false;

        _updateCheckRunning = true;
        try
        {
            UpdateStatusText.Text = "Checking GitHub Releases…";
            var result = await _updateService.CheckLatestAsync();
            AppVersionText.Text = result.CurrentVersion;
            UpdateStatusText.Text = result.Message;

            if (!result.Success || !result.UpdateAvailable)
                return false;

            if (!allowAutomaticInstall || (!_settings.AutoInstallToastifyUpdates && !forceInstall))
                return false;

            if (string.IsNullOrWhiteSpace(result.DownloadUrl))
            {
                UpdateStatusText.Text += " The installer for this architecture is not available.";
                return false;
            }

            UpdateStatusText.Text = $"Downloading and installing Toastify Reloaded {result.LatestVersion}…";
            var started = await _updateService.PrepareAndLaunchUpdateAsync(result, Environment.ProcessId);
            if (!started)
            {
                UpdateStatusText.Text = "The updater could not be started.";
                return false;
            }

            UpdateStatusText.Text = "Update ready. Toastify Reloaded will restart automatically.";
            _reallyExit = true;
            Close();
            return true;
        }
        catch (Exception ex)
        {
            UpdateStatusText.Text = $"Update failed: {ex.Message}";
            return false;
        }
        finally
        {
            _updateCheckRunning = false;
        }
    }

    private async Task RunCompatibilityCheckAsync(bool automatic)
    {
        if (_maintenanceRunning)
            return;

        _maintenanceRunning = true;
        try
        {
            CompatibilityStatusText.Text = "Checking Spotify and Spicetify…";
            var spotifyInfo = await _spotifyInstallation.GetInfoAsync();
            var spicetifyVersion = await _compatibility.GetSpicetifyVersionAsync();

            SpotifyVersionText.Text = spotifyInfo.IsDetected ? $"{spotifyInfo.Version} ({spotifyInfo.InstallKind})" : "Not detected";
            SpicetifyVersionText.Text = string.IsNullOrWhiteSpace(spicetifyVersion) ? "Not detected" : spicetifyVersion;

            if (!spotifyInfo.IsDetected)
            {
                CompatibilityStatusText.Text = "Spotify version not detected. Open Spotify once and retry.";
                return;
            }

            if (string.IsNullOrWhiteSpace(_settings.LastKnownSpotifyVersion))
            {
                _settings.LastKnownSpotifyVersion = spotifyInfo.Version;
                _settingsService.Save(_settings);
                CompatibilityStatusText.Text = $"Baseline registered: Spotify {spotifyInfo.Version}.";
                return;
            }

            if (_settings.LastKnownSpotifyVersion.Equals(spotifyInfo.Version, StringComparison.OrdinalIgnoreCase))
            {
                CompatibilityStatusText.Text = $"Compatible: Spotify {spotifyInfo.Version}.";
                return;
            }

            CompatibilityStatusText.Text = $"Spotify update detected: {_settings.LastKnownSpotifyVersion} → {spotifyInfo.Version}.";

            if (!automatic || !_settings.AutoRepairAfterSpotifyUpdate)
                return;

            if (_settings.LastAutoRepairAttemptVersion.Equals(spotifyInfo.Version, StringComparison.OrdinalIgnoreCase))
            {
                CompatibilityStatusText.Text += " Automatic repair was already attempted for this version.";
                return;
            }

            _settings.LastAutoRepairAttemptVersion = spotifyInfo.Version;
            _settings.LastAutoRepairAttemptUtc = DateTimeOffset.UtcNow;
            _settingsService.Save(_settings);
            await RepairForSpotifyVersionAsync(spotifyInfo.Version, manual: false);
        }
        catch (Exception ex)
        {
            CompatibilityStatusText.Text = $"Compatibility check failed: {ex.Message}";
        }
        finally
        {
            _maintenanceRunning = false;
        }
    }

    private async Task RepairForSpotifyVersionAsync(string spotifyVersion, bool manual)
    {
        CompatibilityStatusText.Text = manual
            ? $"Manual repair for Spotify {spotifyVersion}…"
            : $"New Spotify version {spotifyVersion}: automatic repair…";

        var result = await _compatibility.RepairAsync(_settings);
        SpicetifyVersionText.Text = string.IsNullOrWhiteSpace(result.SpicetifyVersion) ? "Not detected" : result.SpicetifyVersion;

        if (result.Success)
        {
            _settings.LastKnownSpotifyVersion = spotifyVersion;
            _settings.LastAutoRepairAttemptVersion = spotifyVersion;
            _settings.LastAutoRepairAttemptUtc = DateTimeOffset.UtcNow;
            _settingsService.Save(_settings);
            CompatibilityStatusText.Text = $"✓ {result.Message}";
            await Task.Delay(1000);
            await RefreshSpotifyStatusAsync();
        }
        else
        {
            CompatibilityStatusText.Text = $"Repair failed: {result.Message} No automatic repair loop will be started for Spotify {spotifyVersion}.";
        }
    }

    private void LstHotKeys_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LstHotKeys.SelectedItem is not HotkeyBinding binding)
            return;

        _selectedHotkey = binding;
        LoadHotkeyEditor(binding);
    }

    private void LoadHotkeyEditor(HotkeyBinding binding)
    {
        _loadingHotkeyEditor = true;
        try
        {
            var parts = binding.Shortcut.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            HotkeyCtrlCheckBox.IsChecked = parts[..Math.Max(0, parts.Length - 1)].Any(p => p.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) || p.Equals("Control", StringComparison.OrdinalIgnoreCase));
            HotkeyAltCheckBox.IsChecked = parts[..Math.Max(0, parts.Length - 1)].Any(p => p.Equals("Alt", StringComparison.OrdinalIgnoreCase));
            HotkeyShiftCheckBox.IsChecked = parts[..Math.Max(0, parts.Length - 1)].Any(p => p.Equals("Shift", StringComparison.OrdinalIgnoreCase));
            HotkeyWinCheckBox.IsChecked = parts[..Math.Max(0, parts.Length - 1)].Any(p => p.Equals("Win", StringComparison.OrdinalIgnoreCase) || p.Equals("Windows", StringComparison.OrdinalIgnoreCase));
            TxtSingleKey.Text = parts.Length > 0 ? parts[^1] : string.Empty;
            HotkeyValidityText.Text = string.Empty;
            HotkeyValidityGrid.Visibility = Visibility.Collapsed;
        }
        finally
        {
            _loadingHotkeyEditor = false;
        }
    }

    private void HotkeyEditor_Changed(object sender, RoutedEventArgs e) => ApplyHotkeyEditorToSelected();
    private void HotkeyEditor_TextChanged(object sender, TextChangedEventArgs e) => ApplyHotkeyEditorToSelected();

    private void ApplyHotkeyEditorToSelected()
    {
        if (_loadingHotkeyEditor || _selectedHotkey is null)
            return;

        var parts = new List<string>();
        if (HotkeyCtrlCheckBox.IsChecked == true) parts.Add("Ctrl");
        if (HotkeyAltCheckBox.IsChecked == true) parts.Add("Alt");
        if (HotkeyShiftCheckBox.IsChecked == true) parts.Add("Shift");
        if (HotkeyWinCheckBox.IsChecked == true) parts.Add("Win");
        if (!string.IsNullOrWhiteSpace(TxtSingleKey.Text)) parts.Add(TxtSingleKey.Text.Trim());

        _selectedHotkey.Shortcut = string.Join("+", parts);
        HotkeyValidityText.Text = string.Empty;
        HotkeyValidityGrid.Visibility = Visibility.Collapsed;
        LstHotKeys.Items.Refresh();
    }

    private void TxtSingleKey_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
        {
            e.Handled = true;
            return;
        }

        TxtSingleKey.Text = key.ToString();
        TxtSingleKey.CaretIndex = TxtSingleKey.Text.Length;
        e.Handled = true;
    }

    private void ToastEnabledState_Changed(object sender, RoutedEventArgs e) => UpdateToastOptionsEnabledState();

    private void UpdateToastOptionsEnabledState()
    {
        if (ToastOptionsTabControl is not null)
            ToastOptionsTabControl.IsEnabled = CbDisableToast.IsChecked != true;
    }

    private void SelectTrackFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Select track output file",
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            FileName = string.IsNullOrWhiteSpace(_settings.TrackFilePath) ? "current-track.txt" : Path.GetFileName(_settings.TrackFilePath)
        };
        if (dialog.ShowDialog(this) == true)
        {
            _settings.TrackFilePath = dialog.FileName;
            CbSaveTrackToFile.IsChecked = true;
        }
    }

    private void SaveAll_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveSettingsFromUi();
            RegisterHotkeys(showSuccess: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Settings not saved", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ResetCurrentTab_Click(object sender, RoutedEventArgs e)
    {
        var defaults = new AppSettings();
        switch (TabControl1.SelectedItem)
        {
            case TabItem tab when tab == Hotkeys:
                _hotkeys.Clear();
                foreach (var item in AppSettings.CreateDefaultHotkeys()) _hotkeys.Add(item);
                LstHotKeys.SelectedIndex = _hotkeys.Count > 0 ? 0 : -1;
                break;
            case TabItem tab when tab == TabToast:
                _settings.ShowToastOnTrackChange = defaults.ShowToastOnTrackChange;
                _settings.OnlyShowToastOnHotkey = defaults.OnlyShowToastOnHotkey;
                _settings.ToastDurationMs = defaults.ToastDurationMs;
                _settings.ToastWidth = defaults.ToastWidth;
                _settings.ToastHeight = defaults.ToastHeight;
                _settings.PositionLeft = defaults.PositionLeft;
                _settings.PositionTop = defaults.PositionTop;
                _settings.ToastBorderThickness = defaults.ToastBorderThickness;
                _settings.ToastCornerTopLeft = defaults.ToastCornerTopLeft;
                _settings.ToastCornerTopRight = defaults.ToastCornerTopRight;
                _settings.ToastCornerBottomLeft = defaults.ToastCornerBottomLeft;
                _settings.ToastCornerBottomRight = defaults.ToastCornerBottomRight;
                _settings.ToastColorTop = defaults.ToastColorTop;
                _settings.ToastColorBottom = defaults.ToastColorBottom;
                _settings.ToastBorderColor = defaults.ToastBorderColor;
                _settings.ToastTitle1Color = defaults.ToastTitle1Color;
                _settings.ToastTitle2Color = defaults.ToastTitle2Color;
                _settings.ToastTitle1FontSize = defaults.ToastTitle1FontSize;
                _settings.ToastTitle2FontSize = defaults.ToastTitle2FontSize;
                _settings.ToastTitle1DropShadow = defaults.ToastTitle1DropShadow;
                _settings.ToastTitle2DropShadow = defaults.ToastTitle2DropShadow;
                _settings.ToastTitle1ShadowDepth = defaults.ToastTitle1ShadowDepth;
                _settings.ToastTitle1ShadowBlur = defaults.ToastTitle1ShadowBlur;
                _settings.ToastTitle2ShadowDepth = defaults.ToastTitle2ShadowDepth;
                _settings.ToastTitle2ShadowBlur = defaults.ToastTitle2ShadowBlur;
                _settings.SongProgressBarBackgroundColor = defaults.SongProgressBarBackgroundColor;
                _settings.SongProgressBarForegroundColor = defaults.SongProgressBarForegroundColor;
                LoadSettingsIntoUi();
                break;
            case TabItem tab when tab == TabAdvanced:
                CbUseProxy.IsChecked = false;
                ProxyHostTextBox.Text = string.Empty;
                ProxyPortTextBox.Text = string.Empty;
                ProxyUsernameTextBox.Text = string.Empty;
                CbBypassProxyLocal.IsChecked = false;
                CbEnableSpotifyWebApi.IsChecked = false;
                CbEnableBroadcaster.IsChecked = false;
                break;
            case TabItem tab when tab == TabReloaded:
                AutoCheckUpdatesCheckBox.IsChecked = true;
                AutoInstallUpdatesCheckBox.IsChecked = true;
                AutoRepairSpotifyCheckBox.IsChecked = true;
                KeepLyricsCheckBox.IsChecked = true;
                AutoUpgradeSpicetifyCheckBox.IsChecked = true;
                RestartSpotifyCheckBox.IsChecked = true;
                break;
            default:
                StartWithWindowsCheckBox.IsChecked = false;
                MinimizeSpotifyCheckBox.IsChecked = false;
                CloseSpotifyCheckBox.IsChecked = false;
                WindowsMixerIncrementUpDown.Value = 1.0;
                TbClipboardTemplate.Text = "{0}";
                CbSaveTrackToFile.IsChecked = false;
                CbAnalytics.IsChecked = false;
                break;
        }
    }

    private void ResetAll_Click(object sender, RoutedEventArgs e)
    {
        _settings = new AppSettings();
        _hotkeys.Clear();
        foreach (var item in AppSettings.CreateDefaultHotkeys()) _hotkeys.Add(item);
        LstHotKeys.SelectedIndex = _hotkeys.Count > 0 ? 0 : -1;
        LoadSettingsIntoUi();
        LoadMaintenanceSettingsIntoUi();
    }

    private void SaveMaintenance_Click(object sender, RoutedEventArgs e)
    {
        SaveMaintenanceSettingsFromUi();
        _settingsService.Save(_settings);
        UpdateStatusText.Text = "Update and Compatibility Guard settings saved.";
    }

    private async void CheckUpdatesNow_Click(object sender, RoutedEventArgs e) =>
        await CheckForToastifyUpdatesAsync(allowAutomaticInstall: false);

    private async void InstallUpdateNow_Click(object sender, RoutedEventArgs e) =>
        await CheckForToastifyUpdatesAsync(allowAutomaticInstall: true, forceInstall: true);

    private async void CheckCompatibilityNow_Click(object sender, RoutedEventArgs e) =>
        await RunCompatibilityCheckAsync(automatic: false);

    private async void RepairSpotifyNow_Click(object sender, RoutedEventArgs e)
    {
        if (_maintenanceRunning)
            return;

        _maintenanceRunning = true;
        try
        {
            SaveMaintenanceSettingsFromUi();
            _settingsService.Save(_settings);
            var info = await _spotifyInstallation.GetInfoAsync();
            if (!info.IsDetected)
            {
                CompatibilityStatusText.Text = "Cannot repair: Spotify version not detected.";
                return;
            }

            await RepairForSpotifyVersionAsync(info.Version, manual: true);
        }
        finally
        {
            _maintenanceRunning = false;
        }
    }

    private async void TestToast_Click(object sender, RoutedEventArgs e) => await ShowCurrentToastAsync();

    private void InstallLyrics_Click(object sender, RoutedEventArgs e)
    {
        try { PowerShellService.RunScript("install-lyrics.ps1", "-InstallSpicetifyIfMissing"); }
        catch (Exception ex) { ShowScriptError(ex); }
    }

    private void RestoreLyrics_Click(object sender, RoutedEventArgs e)
    {
        try { PowerShellService.RunScript("restore-after-spotify-update.ps1"); }
        catch (Exception ex) { ShowScriptError(ex); }
    }

    private void RemoveLyrics_Click(object sender, RoutedEventArgs e)
    {
        try { PowerShellService.RunScript("remove-lyrics.ps1"); }
        catch (Exception ex) { ShowScriptError(ex); }
    }

    private void Diagnose_Click(object sender, RoutedEventArgs e)
    {
        try { PowerShellService.RunScript("diagnose.ps1"); }
        catch (Exception ex) { ShowScriptError(ex); }
    }

    private void OpenConfigFolder_Click(object sender, RoutedEventArgs e)
    {
        Directory.CreateDirectory(_settingsService.SettingsDirectory);
        Process.Start(new ProcessStartInfo("explorer.exe", _settingsService.SettingsDirectory) { UseShellExecute = true });
    }

    private static void ShowScriptError(Exception ex) =>
        MessageBox.Show(ex.Message, "Unable to start helper", MessageBoxButton.OK, MessageBoxImage.Error);

    private void TryMinimizeSpotify()
    {
        try
        {
            foreach (var process in Process.GetProcessesByName("Spotify"))
            {
                if (process.MainWindowHandle != IntPtr.Zero)
                    NativeMethods.ShowWindow(process.MainWindowHandle, NativeMethods.SW_MINIMIZE);
            }
        }
        catch { }
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitApplication()
    {
        _reallyExit = true;
        Close();
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (!_reallyExit)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        if (_settings.CloseSpotifyWithToastify)
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("Spotify"))
                    process.CloseMainWindow();
            }
            catch { }
        }

        _pollTimer.Stop();
        _compatibilityTimer.Stop();
        _updateTimer.Stop();
        _globalHotkeys?.Dispose();
        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
    }
    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    private static ImageSource? LoadSystemIcon(int resourceId)
    {
        // LoadIcon returns shared system icons, matching the native glyphs used
        // by classic Toastify on the current Windows theme.
        var handle = LoadIcon(IntPtr.Zero, new IntPtr(resourceId));
        if (handle == IntPtr.Zero)
            return null;

        return Imaging.CreateBitmapSourceFromHIcon(handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
    }

}
