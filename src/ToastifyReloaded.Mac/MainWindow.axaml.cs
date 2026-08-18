using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using ToastifyReloaded.Mac.Models;
using ToastifyReloaded.Mac.Services;

namespace ToastifyReloaded.Mac;

public partial class MainWindow : Window
{
    private readonly ProcessService _processService = new();
    private readonly MacSettingsService _settingsService = new();
    private readonly ArtworkService _artworkService = new();
    private readonly LocalizationService _localization = new();
    private readonly MacUpdateService _updates = new();
    private readonly MacUpdateInstallerService _updateInstaller;

    private readonly SpotifyAppleScriptService _spotify;
    private readonly MacGlobalHotkeyService _hotkeys;
    private readonly SpicetifyMacService _spicetify;
    private readonly MacSpotifyVersionService _spotifyVersion;
    private readonly CompatibilityGuardService _compatibilityGuard;
    private readonly MacAutostartService _autostart = new();

    private MacSettings _settings = new();
    private DispatcherTimer? _pollTimer;
    private string _lastTrackIdentity = string.Empty;
    private bool _polling;
    private bool _loadingUi;
    private bool _allowExit;
    private MacUpdateInfo? _availableUpdate;

    public MainWindow()
    {
        InitializeComponent();

        _spotify = new SpotifyAppleScriptService(_processService);
        _updateInstaller = new MacUpdateInstallerService(_processService);
        _hotkeys = new MacGlobalHotkeyService(_spotify);

        _spicetify = new SpicetifyMacService(_processService);
        _spotifyVersion = new MacSpotifyVersionService(_processService);
        _compatibilityGuard = new CompatibilityGuardService(
            _spotifyVersion,
            _spicetify);

        Opened += async (_, _) => await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        _settings = await _settingsService.LoadAsync();

        PopulateMonitorCombo();
        LoadSettingsToUi();
        ApplyApplicationTheme();
        ApplyLocalization();

        await ApplyHotkeysAsync();

        if (_settings.EnableCompatibilityGuard)
            await RunCompatibilityGuardAsync(silent: true);

        await RefreshStatusAsync();

        if (_settings.AutoCheckMacUpdates)
        {
            _ = CheckUpdatesAsync(
                silent: true,
                installIfAvailable: _settings.AutoInstallMacUpdates);
        }

        _pollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _pollTimer.Tick += async (_, _) => await PollSpotifyAsync();
        _pollTimer.Start();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_allowExit && _settings.CloseToTray)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }

        _pollTimer?.Stop();
        _hotkeys.Dispose();

        base.OnClosing(e);
    }

    public void ShowFromTray()
    {
        ShowInTaskbar = true;

        if (!IsVisible)
            Show();

        WindowState = WindowState.Normal;
        Activate();
    }

    private void HideToTray()
    {
        ShowInTaskbar = false;
        Hide();
    }

    public Task TrayPlayPauseAsync() => _spotify.PlayPauseAsync();
    public Task TrayNextAsync() => _spotify.NextAsync();
    public Task TrayPreviousAsync() => _spotify.PreviousAsync();

    public void RequestExit()
    {
        _allowExit = true;
        _pollTimer?.Stop();
        _hotkeys.Dispose();

        if (Application.Current?.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private async Task PollSpotifyAsync()
    {
        if (_polling)
            return;

        _polling = true;

        try
        {
            var track = await _spotify.GetTrackAsync();
            if (track is null || string.IsNullOrWhiteSpace(track.Title))
                return;

            if (_settings.ShowToastOnTrackChange &&
                track.Identity != _lastTrackIdentity)
            {
                _lastTrackIdentity = track.Identity;
                _ = ShowToastAsync(track);
            }
        }
        finally
        {
            _polling = false;
        }
    }

    private async Task ShowToastAsync(MacTrackInfo track)
    {
        try
        {
            var toast = new ToastWindow(
                track,
                _settings,
                _artworkService);

            await toast.ShowToastAsync();
        }
        catch
        {
            // A toast failure must never terminate the background application.
        }
    }

    private async void Save_Click(object? sender, RoutedEventArgs e)
    {
        _settings = ReadSettingsFromUi();

        await _settingsService.SaveAsync(_settings);
        await _autostart.SetEnabledAsync(_settings.StartWithSession);

        ApplyApplicationTheme();
        ApplyLocalization();

        await ApplyHotkeysAsync();
        await RefreshStatusAsync();
    }

    private async void TestToast_Click(object? sender, RoutedEventArgs e)
    {
        _settings = ReadSettingsFromUi();

        var track = await _spotify.GetTrackAsync()
            ?? new MacTrackInfo(
                "Toastify Reloaded macOS",
                "macOS 1.5.0 Preview 1",
                "Classic Toastify experience",
                string.Empty,
                76,
                200,
                true);

        await ShowToastAsync(track);
    }

    private async void RefreshStatus_Click(object? sender, RoutedEventArgs e)
        => await RefreshStatusAsync();

    private async void EnableLyrics_Click(object? sender, RoutedEventArgs e)
    {
        var result = await _spicetify.EnableLyricsAsync();

        Find<TextBlock>("SpicetifyStatusText").Text =
            $"Spicetify: {result.Message}";
    }

    private async void RepairSpicetify_Click(object? sender, RoutedEventArgs e)
    {
        var result = await _spicetify.RepairAfterSpotifyUpdateAsync(
            _settings.KeepLyricsPlus);

        Find<TextBlock>("SpicetifyStatusText").Text =
            $"Spicetify: {result.Message}";
    }

    private void Exit_Click(object? sender, RoutedEventArgs e)
        => RequestExit();

    private void LanguageCombo_SelectionChanged(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (_loadingUi)
            return;

        _settings.Language = ComboValue(
            "LanguageCombo",
            "Italiano");

        var selectedMonitor = ComboValue("MonitorCombo", "-1");
        ApplyLocalization();
        PopulateMonitorCombo();
        SelectCombo("MonitorCombo", selectedMonitor);
    }

    private async void ExportSettings_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _settings = ReadSettingsFromUi();

            var file = await StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title = _settings.Language == "English"
                        ? "Export Toastify Reloaded settings"
                        : "Esporta impostazioni Toastify Reloaded",
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType("JSON")
                        {
                            Patterns = new[] { "*.json" }
                        }
                    }
                });

            if (file is null)
                return;

            await using var stream = await file.OpenWriteAsync();
            stream.SetLength(0);

            await _settingsService.ExportAsync(
                stream,
                _settings);

            Find<TextBlock>("SettingsIoStatusText").Text =
                T("ExportOk");
        }
        catch (Exception ex)
        {
            Find<TextBlock>("SettingsIoStatusText").Text =
                ex.Message;
        }
    }

    private async void ImportSettings_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var files = await StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = _settings.Language == "English"
                        ? "Import Toastify Reloaded settings"
                        : "Importa impostazioni Toastify Reloaded",
                    AllowMultiple = false
                });

            if (files.Count == 0)
                return;

            await using var stream = await files[0].OpenReadAsync();
            _settings = await _settingsService.ImportAsync(stream);

            LoadSettingsToUi();
            ApplyApplicationTheme();
            ApplyLocalization();

            await _settingsService.SaveAsync(_settings);
            await _autostart.SetEnabledAsync(_settings.StartWithSession);
            await ApplyHotkeysAsync();
            await RefreshStatusAsync();

            Find<TextBlock>("SettingsIoStatusText").Text =
                T("ImportOk");
        }
        catch (Exception ex)
        {
            Find<TextBlock>("SettingsIoStatusText").Text =
                ex.Message;
        }
    }

    private async void CheckUpdates_Click(object? sender, RoutedEventArgs e)
        => await CheckUpdatesAsync(
            silent: false,
            installIfAvailable: false);

    private async Task CheckUpdatesAsync(bool silent, bool installIfAvailable)
    {
        try
        {
            _availableUpdate = await _updates.CheckAsync();

            if (_availableUpdate is null)
            {
                Find<Button>("OpenReleaseButton").IsEnabled = false;
                Find<Button>("InstallUpdateButton").IsEnabled = false;

                if (!silent)
                    Find<TextBlock>("UpdateStatusText").Text = T("NoUpdate");

                return;
            }

            Find<Button>("OpenReleaseButton").IsEnabled = true;
            Find<Button>("InstallUpdateButton").IsEnabled = true;

            Find<TextBlock>("UpdateStatusText").Text =
                $"{T("UpdateAvailable")}: {_availableUpdate.Tag}";

            if (installIfAvailable)
                await InstallAvailableUpdateAsync();
        }
        catch (Exception ex)
        {
            if (!silent)
                Find<TextBlock>("UpdateStatusText").Text = ex.Message;
        }
    }


    private async void InstallUpdate_Click(object? sender, RoutedEventArgs e)
        => await InstallAvailableUpdateAsync();

    private async Task InstallAvailableUpdateAsync()
    {
        if (_availableUpdate is null)
            return;

        Find<Button>("InstallUpdateButton").IsEnabled = false;

        try
        {
            Find<TextBlock>("UpdateStatusText").Text =
                _settings.Language == "English"
                    ? "Downloading update..."
                    : "Download aggiornamento...";

            var result =
                await _updateInstaller.DownloadAndApplyAsync(
                    _availableUpdate);

            Find<TextBlock>("UpdateStatusText").Text =
                result.Message;

            if (result.Success &&
                result.RestartStarted)
            {
                RequestExit();
            }
        }
        catch (Exception ex)
        {
            Find<TextBlock>("UpdateStatusText").Text =
                ex.Message;
        }
        finally
        {
            if (_availableUpdate is not null)
                Find<Button>("InstallUpdateButton").IsEnabled = true;
        }
    }

    private async void OpenLatestRelease_Click(object? sender, RoutedEventArgs e)
    {
        if (_availableUpdate is null)
            return;

        await Launcher.LaunchUriAsync(
            _availableUpdate.ReleaseUri);
    }

    private async void RunCompatibilityGuard_Click(
        object? sender,
        RoutedEventArgs e)
        => await RunCompatibilityGuardAsync(silent: false);

    private async Task RunCompatibilityGuardAsync(bool silent)
    {
        try
        {
            _settings = ReadSettingsFromUi();

            var result = await _compatibilityGuard.CheckAsync(_settings);

            await _settingsService.SaveAsync(_settings);

            var detail = result.Spotify is null
                ? ""
                : $" ({result.Spotify.Source}: {result.Spotify.Version})";

            Find<TextBlock>("CompatibilityGuardStatusText").Text =
                T(result.Message) + detail;
        }
        catch (Exception ex)
        {
            if (!silent)
                Find<TextBlock>("CompatibilityGuardStatusText").Text =
                    ex.Message;
        }
    }

    private async Task RefreshStatusAsync()
    {
        var appleScriptAvailable = await _spotify.IsAvailableAsync();
        var spotifyAvailable = appleScriptAvailable &&
                               await _spotify.IsSpotifyAvailableAsync();

        var spicetifyVersion = await _spicetify.GetVersionAsync();
        var spotifyInstall = await _spotifyVersion.GetVersionAsync();

        Find<TextBlock>("PlayerctlStatusText").Text =
            $"AppleScript / osascript: {(appleScriptAvailable ? "OK" : T("Missing"))}";

        Find<TextBlock>("SpotifyStatusText").Text =
            $"Spotify Automation: {(spotifyAvailable ? T("Detected") : T("NotDetected"))}";

        var hotkeyPermission = !_settings.EnableGlobalHotkeys || _hotkeys.PermissionGranted;
        Find<TextBlock>("SessionStatusText").Text =
            _settings.EnableGlobalHotkeys
                ? $"Accessibility hotkeys: {(hotkeyPermission ? T("Granted") : T("Required"))}"
                : T("HotkeysDisabled");

        Find<TextBlock>("SpicetifyStatusText").Text =
            $"Spicetify: {spicetifyVersion}";

        var hotkeyText = _settings.EnableGlobalHotkeys
            ? (hotkeyPermission ? T("HotkeysReady") : T("AccessibilityRequired"))
            : T("HotkeysDisabled");

        Find<TextBlock>("StatusText").Text =
            spotifyAvailable
                ? $"{T("SpotifyReady")} • {hotkeyText}"
                : $"{T("SpotifyMissing")} • {hotkeyText}";

        Find<TextBlock>("DiagnosticsText").Text =
            string.Join(
                Environment.NewLine,
                new[]
                {
                    "Toastify Reloaded macOS: 1.5.0 Preview 1",
                    $"OS: {Environment.OSVersion}",
                    $"Architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}",
                    $"AppleScript / osascript: {(appleScriptAvailable ? "available" : "missing")}",
                    $"Spotify Automation: {(spotifyAvailable ? "available" : "not detected")}",
                    $"Accessibility hotkeys: {(hotkeyPermission ? "granted/not required" : "required")}",
                    $"Hotkey bindings: {_hotkeys.BindingCount}",
                    $"Spotify install: {(spotifyInstall is null ? "unknown" : spotifyInstall.Source + " " + spotifyInstall.Version)}",
                    $"Spicetify: {spicetifyVersion}",
                    $"Config: {_settingsService.SettingsPath}",
                    $"App bundle: {MacUpdateInstallerService.FindCurrentAppBundle() ?? "development / unpacked"}",
                    $"Updater: {MacUpdateService.CurrentTag}"
                });
    }

    private async Task ApplyHotkeysAsync()
    {
        var result = await _hotkeys.ApplyAsync(_settings);
        Find<TextBlock>("StatusText").Text = result.Message;
    }

    private void ApplyApplicationTheme()
    {
        if (Application.Current is null)
            return;

        Application.Current.RequestedThemeVariant =
            _settings.ApplicationTheme switch
            {
                "Dark" => ThemeVariant.Dark,
                "Light" => ThemeVariant.Light,
                _ => ThemeVariant.Default
            };
    }

    private void ApplyLocalization()
    {
        _localization.Apply(this, _settings.Language);
        Title = "Toastify Reloaded — macOS";
    }

    private string T(string key)
        => _localization.Get(key, _settings.Language);

    private void LoadSettingsToUi()
    {
        _loadingUi = true;

        try
        {
            SelectCombo("AppThemeCombo", _settings.ApplicationTheme);
            SelectCombo("LanguageCombo", _settings.Language);

            SetCheck("AutostartCheck", _settings.StartWithSession);
            SetCheck("CloseToTrayCheck", _settings.CloseToTray);
            SetCheck("EnableHotkeysCheck", _settings.EnableGlobalHotkeys);

            SetText("PlayPauseHotkeyBox", _settings.HotkeyPlayPause);
            SetText("NextHotkeyBox", _settings.HotkeyNext);
            SetText("PreviousHotkeyBox", _settings.HotkeyPrevious);
            SetText("VolumeUpHotkeyBox", _settings.HotkeyVolumeUp);
            SetText("VolumeDownHotkeyBox", _settings.HotkeyVolumeDown);
            SetText("MuteHotkeyBox", _settings.HotkeyMute);
            SetText("SeekForwardHotkeyBox", _settings.HotkeySeekForward);
            SetText("SeekBackwardHotkeyBox", _settings.HotkeySeekBackward);

            SetCheck("ShowToastCheck", _settings.ShowToastOnTrackChange);
            SetNumeric("ToastDisplayNumeric", _settings.ToastDisplayMs);
            SetNumeric("FadeInNumeric", _settings.FadeInMs);
            SetNumeric("FadeOutNumeric", _settings.FadeOutMs);
            SetCheck("ShowProgressCheck", _settings.ShowProgress);
            SetCheck("ShowTimeCheck", _settings.ShowSongTime);
            SetCheck("AutoWidthCheck", _settings.AutoWidth);
            SetNumeric("MinWidthNumeric", _settings.MinWidth);
            SetNumeric("MaxWidthNumeric", _settings.MaxWidth);
            SelectCombo("ImageModeCombo", _settings.ImageMode);
            SetCheck("IconFallbackCheck", _settings.IconFallback);
            SelectCombo("ToastThemeCombo", _settings.ToastTheme);

            SelectCombo("ToastFontCombo", _settings.ToastFontFamily);
            SetNumeric("TitleFontSizeNumeric", (decimal)_settings.TitleFontSize);
            SetNumeric("ArtistFontSizeNumeric", (decimal)_settings.ArtistFontSize);
            SetNumeric("TimeFontSizeNumeric", (decimal)_settings.TimeFontSize);

            SetText("CustomTopColorBox", _settings.CustomTopColor);
            SetText("CustomBottomColorBox", _settings.CustomBottomColor);
            SetText("CustomBorderColorBox", _settings.CustomBorderColor);
            SetText("CustomTitleColorBox", _settings.CustomTitleColor);
            SetText("CustomSecondaryColorBox", _settings.CustomSecondaryColor);
            SetText("CustomProgressBackgroundColorBox", _settings.CustomProgressBackgroundColor);
            SetText("CustomProgressForegroundColorBox", _settings.CustomProgressForegroundColor);

            SelectCombo("MonitorCombo", _settings.MonitorIndex.ToString());
            SelectCombo("ToastPositionCombo", _settings.ToastPosition);
            SetNumeric("ToastMarginXNumeric", _settings.ToastMarginX);
            SetNumeric("ToastMarginYNumeric", _settings.ToastMarginY);

            SelectCombo("AnimationStyleCombo", _settings.AnimationStyle);
            SelectCombo("SlideInCombo", _settings.SlideInDirection);
            SelectCombo("SlideOutCombo", _settings.SlideOutDirection);
            SetNumeric("SlideInDistanceNumeric", _settings.SlideInDistance);
            SetNumeric("SlideOutDistanceNumeric", _settings.SlideOutDistance);

            SetCheck("EnableCompatibilityGuardCheck", _settings.EnableCompatibilityGuard);
            SetCheck("AutoRepairSpicetifyCheck", _settings.AutoRepairSpicetify);
            SetCheck("AutoCheckUpdatesCheck", _settings.AutoCheckMacUpdates);
            SetCheck("AutoInstallUpdatesCheck", _settings.AutoInstallMacUpdates);
        }
        finally
        {
            _loadingUi = false;
        }
    }

    private MacSettings ReadSettingsFromUi()
    {
        return new MacSettings
        {
            ApplicationTheme = ComboValue("AppThemeCombo", "System"),
            Language = ComboValue("LanguageCombo", "Italiano"),
            StartWithSession = CheckValue("AutostartCheck"),
            CloseToTray = CheckValue("CloseToTrayCheck"),

            EnableGlobalHotkeys = CheckValue("EnableHotkeysCheck"),

            HotkeyPlayPause = TextValue("PlayPauseHotkeyBox"),
            HotkeyNext = TextValue("NextHotkeyBox"),
            HotkeyPrevious = TextValue("PreviousHotkeyBox"),
            HotkeyVolumeUp = TextValue("VolumeUpHotkeyBox"),
            HotkeyVolumeDown = TextValue("VolumeDownHotkeyBox"),
            HotkeyMute = TextValue("MuteHotkeyBox"),
            HotkeySeekForward = TextValue("SeekForwardHotkeyBox"),
            HotkeySeekBackward = TextValue("SeekBackwardHotkeyBox"),

            ShowToastOnTrackChange = CheckValue("ShowToastCheck"),
            ToastDisplayMs = NumericValue("ToastDisplayNumeric", 3500),
            FadeInMs = NumericValue("FadeInNumeric", 250),
            FadeOutMs = NumericValue("FadeOutNumeric", 250),
            ShowProgress = CheckValue("ShowProgressCheck"),
            ShowSongTime = CheckValue("ShowTimeCheck"),
            AutoWidth = CheckValue("AutoWidthCheck"),
            MinWidth = NumericValue("MinWidthNumeric", 250),
            MaxWidth = NumericValue("MaxWidthNumeric", 600),
            ImageMode = ComboValue("ImageModeCombo", "Album cover"),
            IconFallback = CheckValue("IconFallbackCheck"),
            ToastTheme = ComboValue("ToastThemeCombo", "Classic Toastify"),

            ToastFontFamily = ComboValue("ToastFontCombo", "Inter"),
            TitleFontSize = NumericDoubleValue("TitleFontSizeNumeric", 15),
            ArtistFontSize = NumericDoubleValue("ArtistFontSizeNumeric", 12),
            TimeFontSize = NumericDoubleValue("TimeFontSizeNumeric", 10),

            CustomTopColor = TextValue("CustomTopColorBox"),
            CustomBottomColor = TextValue("CustomBottomColorBox"),
            CustomBorderColor = TextValue("CustomBorderColorBox"),
            CustomTitleColor = TextValue("CustomTitleColorBox"),
            CustomSecondaryColor = TextValue("CustomSecondaryColorBox"),
            CustomProgressBackgroundColor = TextValue("CustomProgressBackgroundColorBox"),
            CustomProgressForegroundColor = TextValue("CustomProgressForegroundColorBox"),

            AnimationStyle = ComboValue("AnimationStyleCombo", "Fade + Slide"),
            SlideInDirection = ComboValue("SlideInCombo", "Up"),
            SlideOutDirection = ComboValue("SlideOutCombo", "Right"),
            SlideInDistance = NumericValue("SlideInDistanceNumeric", 28),
            SlideOutDistance = NumericValue("SlideOutDistanceNumeric", 50),

            MonitorIndex = int.TryParse(
                ComboValue("MonitorCombo", "-1"),
                out var monitorIndex)
                ? monitorIndex
                : -1,
            ToastPosition = ComboValue("ToastPositionCombo", "BottomRight"),
            ToastMarginX = NumericValue("ToastMarginXNumeric", 18),
            ToastMarginY = NumericValue("ToastMarginYNumeric", 18),

            KeepLyricsPlus = _settings.KeepLyricsPlus,

            EnableCompatibilityGuard = CheckValue("EnableCompatibilityGuardCheck"),
            AutoRepairSpicetify = CheckValue("AutoRepairSpicetifyCheck"),
            LastSpotifyVersion = _settings.LastSpotifyVersion,
            LastRepairAttemptVersion = _settings.LastRepairAttemptVersion,

            AutoCheckMacUpdates = CheckValue("AutoCheckUpdatesCheck"),
            AutoInstallMacUpdates = CheckValue("AutoInstallUpdatesCheck")
        };
    }

    private T Find<T>(string name) where T : Control
        => this.FindControl<T>(name)
           ?? throw new InvalidOperationException($"Control not found: {name}");


    private void PopulateMonitorCombo()
    {
        var combo = Find<ComboBox>("MonitorCombo");
        combo.Items.Clear();

        combo.Items.Add(
            new ComboBoxItem
            {
                Content = _settings.Language == "English"
                    ? "Primary monitor"
                    : "Monitor principale",
                Tag = "-1"
            });

        for (var i = 0; i < Screens.All.Count; i++)
        {
            var screen = Screens.All[i];
            var displayName = string.IsNullOrWhiteSpace(screen.DisplayName)
                ? $"Monitor {i + 1}"
                : screen.DisplayName;

            var primary = screen.IsPrimary
                ? (_settings.Language == "English" ? " · primary" : " · principale")
                : "";

            combo.Items.Add(
                new ComboBoxItem
                {
                    Content =
                        $"{i + 1}: {displayName} — " +
                        $"{screen.Bounds.Width}×{screen.Bounds.Height}" +
                        primary,
                    Tag = i.ToString()
                });
        }
    }

    private void SelectCombo(string name, string value)
    {
        var combo = Find<ComboBox>(name);

        foreach (var item in combo.Items)
        {
            if (item is not ComboBoxItem comboItem)
                continue;

            var canonical =
                comboItem.Tag?.ToString()
                ?? comboItem.Content?.ToString();

            if (string.Equals(
                    canonical,
                    value,
                    StringComparison.OrdinalIgnoreCase))
            {
                combo.SelectedItem = item;
                return;
            }
        }

        combo.SelectedIndex = 0;
    }

    private string ComboValue(string name, string fallback)
    {
        var combo = Find<ComboBox>(name);

        if (combo.SelectedItem is not ComboBoxItem item)
            return fallback;

        return item.Tag?.ToString()
               ?? item.Content?.ToString()
               ?? fallback;
    }

    private void SetCheck(string name, bool value)
        => Find<CheckBox>(name).IsChecked = value;

    private bool CheckValue(string name)
        => Find<CheckBox>(name).IsChecked == true;

    private void SetText(string name, string value)
        => Find<TextBox>(name).Text = value;

    private string TextValue(string name)
        => Find<TextBox>(name).Text?.Trim() ?? string.Empty;

    private void SetNumeric(string name, decimal value)
        => Find<NumericUpDown>(name).Value = value;


    private double NumericDoubleValue(string name, double fallback)
    {
        var value = Find<NumericUpDown>(name).Value;

        return value.HasValue
            ? (double)value.Value
            : fallback;
    }

    private int NumericValue(string name, int fallback)
    {
        var value = Find<NumericUpDown>(name).Value;

        return value.HasValue
            ? (int)Math.Round(value.Value)
            : fallback;
    }
}
