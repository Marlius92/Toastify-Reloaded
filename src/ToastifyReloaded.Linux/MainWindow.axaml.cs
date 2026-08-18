using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using ToastifyReloaded.Linux.Models;
using ToastifyReloaded.Linux.Services;

namespace ToastifyReloaded.Linux;

public partial class MainWindow : Window
{
    private readonly ProcessService _processService = new();
    private readonly LinuxSettingsService _settingsService = new();
    private readonly ArtworkService _artworkService = new();

    private readonly PlayerctlService _playerctl;
    private readonly GlobalHotkeyCoordinator _hotkeys;
    private readonly SpicetifyLinuxService _spicetify;
    private readonly LinuxAutostartService _autostart = new();

    private LinuxSettings _settings = new();
    private DispatcherTimer? _pollTimer;
    private string _lastTrackIdentity = string.Empty;
    private bool _polling;

    public MainWindow()
    {
        InitializeComponent();

        _playerctl = new PlayerctlService(_processService);
        var x11Hotkeys = new XbindkeysService(_processService, _settingsService);
        var waylandHotkeys = new XdgGlobalShortcutsService(_playerctl);
        _hotkeys = new GlobalHotkeyCoordinator(x11Hotkeys, waylandHotkeys);
        _spicetify = new SpicetifyLinuxService(_processService);

        Opened += async (_, _) => await InitializeAsync();
        Closing += (_, _) => _hotkeys.Dispose();
    }

    private async Task InitializeAsync()
    {
        _settings = await _settingsService.LoadAsync();
        LoadSettingsToUi();
        ApplyApplicationTheme();

        await ApplyHotkeysAsync();
        await RefreshStatusAsync();

        _pollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _pollTimer.Tick += async (_, _) => await PollSpotifyAsync();
        _pollTimer.Start();
    }

    private async Task PollSpotifyAsync()
    {
        if (_polling)
            return;

        _polling = true;

        try
        {
            var track = await _playerctl.GetTrackAsync();
            if (track is null)
                return;

            if (string.IsNullOrWhiteSpace(track.Title))
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

    private async Task ShowToastAsync(LinuxTrackInfo track)
    {
        try
        {
            var toast = new ToastWindow(track, _settings, _artworkService);
            await toast.ShowToastAsync();
        }
        catch
        {
            // The preview must not crash because a toast failed.
        }
    }

    private async void Save_Click(object? sender, RoutedEventArgs e)
    {
        _settings = ReadSettingsFromUi();
        await _settingsService.SaveAsync(_settings);
        await _autostart.SetEnabledAsync(_settings.StartWithSession);
        ApplyApplicationTheme();
        await ApplyHotkeysAsync();
        await RefreshStatusAsync();
    }

    private async void TestToast_Click(object? sender, RoutedEventArgs e)
    {
        _settings = ReadSettingsFromUi();

        var track = await _playerctl.GetTrackAsync()
            ?? new LinuxTrackInfo(
                "Toastify Reloaded Linux",
                "Linux Preview",
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
        this.FindControl<TextBlock>("SpicetifyStatusText")!.Text =
            $"Spicetify: {result.Message}";
    }

    private async void RepairSpicetify_Click(object? sender, RoutedEventArgs e)
    {
        var result = await _spicetify.RepairAsync();
        this.FindControl<TextBlock>("SpicetifyStatusText")!.Text =
            $"Spicetify: {result.Message}";
    }

    private void Exit_Click(object? sender, RoutedEventArgs e)
    {
        _pollTimer?.Stop();
        _hotkeys.Dispose();

        if (Application.Current?.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private async Task RefreshStatusAsync()
    {
        var playerctlAvailable = await _playerctl.IsAvailableAsync();
        var spotifyAvailable = playerctlAvailable &&
                               await _playerctl.IsSpotifyAvailableAsync();
        var spicetifyVersion = await _spicetify.GetVersionAsync();

        this.FindControl<TextBlock>("PlayerctlStatusText")!.Text =
            $"playerctl: {(playerctlAvailable ? "OK" : "mancante")}";

        this.FindControl<TextBlock>("SpotifyStatusText")!.Text =
            $"Spotify MPRIS: {(spotifyAvailable ? "rilevato" : "non rilevato")}";

        this.FindControl<TextBlock>("SessionStatusText")!.Text =
            $"Sessione grafica: {_hotkeys.SessionType}";

        this.FindControl<TextBlock>("SpicetifyStatusText")!.Text =
            $"Spicetify: {spicetifyVersion}";

        var hotkeyText = _hotkeys.IsWayland
            ? "Wayland: backend XDG Global Shortcuts Portal"
            : "X11: backend hotkey xbindkeys";

        this.FindControl<TextBlock>("StatusText")!.Text =
            spotifyAvailable
                ? $"Spotify pronto • {hotkeyText}"
                : $"Spotify non rilevato • {hotkeyText}";

        this.FindControl<TextBlock>("DiagnosticsText")!.Text =
            string.Join(
                Environment.NewLine,
                new[]
                {
                    $"Toastify Reloaded Linux: 1.4.0-preview.2",
                    $"OS: {Environment.OSVersion}",
                    $"Sessione: {_hotkeys.SessionType}",
                    $"DISPLAY: {Environment.GetEnvironmentVariable("DISPLAY") ?? "(none)"}",
                    $"WAYLAND_DISPLAY: {Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") ?? "(none)"}",
                    $"playerctl: {(playerctlAvailable ? "available" : "missing")}",
                    $"Spotify MPRIS: {(spotifyAvailable ? "available" : "missing")}",
                    $"Spicetify: {spicetifyVersion}",
                    $"Config: {_settingsService.SettingsPath}"
                });
    }

    private async Task ApplyHotkeysAsync()
    {
        var result = await _hotkeys.ApplyAsync(_settings);
        this.FindControl<TextBlock>("StatusText")!.Text = result.Message;
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

    private void LoadSettingsToUi()
    {
        SelectCombo("AppThemeCombo", _settings.ApplicationTheme);
        SelectCombo("LanguageCombo", _settings.Language);
        SetCheck("AutostartCheck", _settings.StartWithSession);
        SetCheck("EnableHotkeysCheck", _settings.EnableX11GlobalHotkeys);

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

        SelectCombo("AnimationStyleCombo", _settings.AnimationStyle);
        SelectCombo("SlideInCombo", _settings.SlideInDirection);
        SelectCombo("SlideOutCombo", _settings.SlideOutDirection);
        SetNumeric("SlideInDistanceNumeric", _settings.SlideInDistance);
        SetNumeric("SlideOutDistanceNumeric", _settings.SlideOutDistance);
    }

    private LinuxSettings ReadSettingsFromUi()
    {
        return new LinuxSettings
        {
            ApplicationTheme = ComboValue("AppThemeCombo", "System"),
            Language = ComboValue("LanguageCombo", "Italiano"),
            StartWithSession = CheckValue("AutostartCheck"),
            EnableX11GlobalHotkeys = CheckValue("EnableHotkeysCheck"),
            EnableWaylandPortalHotkeys = CheckValue("EnableHotkeysCheck"),

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

            AnimationStyle = ComboValue("AnimationStyleCombo", "Fade + Slide"),
            SlideInDirection = ComboValue("SlideInCombo", "Up"),
            SlideOutDirection = ComboValue("SlideOutCombo", "Right"),
            SlideInDistance = NumericValue("SlideInDistanceNumeric", 28),
            SlideOutDistance = NumericValue("SlideOutDistanceNumeric", 50),

            KeepLyricsPlus = _settings.KeepLyricsPlus
        };
    }

    private void SelectCombo(string name, string value)
    {
        var combo = this.FindControl<ComboBox>(name)!;

        foreach (var item in combo.Items)
        {
            if (item is ComboBoxItem comboItem &&
                string.Equals(
                    comboItem.Content?.ToString(),
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
        var combo = this.FindControl<ComboBox>(name)!;
        return combo.SelectedItem is ComboBoxItem item
            ? item.Content?.ToString() ?? fallback
            : fallback;
    }

    private void SetCheck(string name, bool value)
        => this.FindControl<CheckBox>(name)!.IsChecked = value;

    private bool CheckValue(string name)
        => this.FindControl<CheckBox>(name)!.IsChecked == true;

    private void SetText(string name, string value)
        => this.FindControl<TextBox>(name)!.Text = value;

    private string TextValue(string name)
        => this.FindControl<TextBox>(name)!.Text?.Trim() ?? string.Empty;

    private void SetNumeric(string name, decimal value)
        => this.FindControl<NumericUpDown>(name)!.Value = value;

    private int NumericValue(string name, int fallback)
    {
        var value = this.FindControl<NumericUpDown>(name)!.Value;
        return value.HasValue
            ? (int)Math.Round(value.Value)
            : fallback;
    }
}
