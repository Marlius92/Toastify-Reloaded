using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.IO;
using ToastifyReloaded.Models;
using ToastifyReloaded.Services;
using Forms = System.Windows.Forms;

namespace ToastifyReloaded;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService = new();
    private readonly SpotifySessionService _spotify = new();
    private readonly DispatcherTimer _pollTimer;
    private readonly ObservableCollection<HotkeyBinding> _hotkeys = new();
    private AppSettings _settings = new();
    private GlobalHotkeyService? _globalHotkeys;
    private Forms.NotifyIcon? _trayIcon;
    private string? _lastTrackIdentity;
    private bool _reallyExit;

    public MainWindow()
    {
        InitializeComponent();

        _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1400) };
        _pollTimer.Tick += async (_, _) => await PollSpotifyAsync();

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
        HotkeysGrid.ItemsSource = _hotkeys;

        ShowToastCheckBox.IsChecked = _settings.ShowToastOnTrackChange;
        StartWithWindowsCheckBox.IsChecked = _settings.StartWithWindows;
        StartMinimizedCheckBox.IsChecked = _settings.StartMinimized;
        ToastDurationTextBox.Text = _settings.ToastDurationMs.ToString();

        CreateTrayIcon();
        RegisterHotkeys(showSuccess: false);
        await RefreshSpotifyStatusAsync();
        _pollTimer.Start();

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
            Icon = System.Drawing.SystemIcons.Application,
            Visible = true
        };

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Apri Toastify Reloaded", null, (_, _) => ShowFromTray());
        menu.Items.Add("Mostra popup", null, async (_, _) => await ShowCurrentToastAsync());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Esci", null, (_, _) => ExitApplication());
        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => ShowFromTray();
    }

    private async Task PollSpotifyAsync()
    {
        var track = await _spotify.GetCurrentTrackAsync();
        if (track is null)
        {
            CurrentTrackText.Text = "Nessun brano Spotify rilevato";
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
        if (_settings.ShowToastOnTrackChange)
            ShowToast(track);
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
            SpotifyStatusText.Text = $"Errore comando: {ex.Message}";
        }
    }

    private void RegisterHotkeys(bool showSuccess)
    {
        if (_globalHotkeys is null)
            return;

        var errors = _globalHotkeys.RegisterAll(_hotkeys);
        if (errors.Count > 0)
        {
            System.Windows.MessageBox.Show(
                string.Join(Environment.NewLine, errors),
                "Alcune hotkey non sono state registrate",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        else if (showSuccess)
        {
            System.Windows.MessageBox.Show("Hotkey salvate e registrate.", "Toastify Reloaded", MessageBoxButton.OK, MessageBoxImage.Information);
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
            SpotifyStatusText.Text = $"Spotify non disponibile: {ex.Message}";
        }
    }

    private async Task ShowCurrentToastAsync()
    {
        var track = await _spotify.GetCurrentTrackAsync() ?? TrackInfo.Empty;
        ShowToast(track);
    }

    private void ShowToast(TrackInfo track)
    {
        var toast = new ToastWindow(track, _settings.ToastDurationMs);
        toast.ShowTimed();
    }

    private void SaveSettingsFromUi()
    {
        if (!int.TryParse(ToastDurationTextBox.Text, out var duration) || duration < 1000 || duration > 30000)
            throw new InvalidOperationException("La durata del popup deve essere tra 1000 e 30000 ms.");

        _settings.ShowToastOnTrackChange = ShowToastCheckBox.IsChecked == true;
        _settings.StartWithWindows = StartWithWindowsCheckBox.IsChecked == true;
        _settings.StartMinimized = StartMinimizedCheckBox.IsChecked == true;
        _settings.ToastDurationMs = duration;
        _settings.Hotkeys = _hotkeys.ToList();

        StartupService.SetEnabled(_settings.StartWithWindows);
        _settingsService.Save(_settings);
    }

    private void SaveHotkeys_Click(object sender, RoutedEventArgs e)
    {
        HotkeysGrid.CommitEdit();
        HotkeysGrid.CommitEdit();
        try
        {
            SaveSettingsFromUi();
            RegisterHotkeys(showSuccess: true);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Impostazioni non salvate", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ResetHotkeys_Click(object sender, RoutedEventArgs e)
    {
        _hotkeys.Clear();
        foreach (var item in AppSettings.CreateDefaultHotkeys())
            _hotkeys.Add(item);
        RegisterHotkeys(showSuccess: false);
    }

    private void SaveGeneral_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveSettingsFromUi();
            System.Windows.MessageBox.Show("Impostazioni salvate.", "Toastify Reloaded", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Impostazioni non salvate", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void RefreshSpotify_Click(object sender, RoutedEventArgs e) => await RefreshSpotifyStatusAsync();
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
        System.Windows.MessageBox.Show(ex.Message, "Impossibile avviare lo script", MessageBoxButton.OK, MessageBoxImage.Error);

    private void HideToTray_Click(object sender, RoutedEventArgs e) => Hide();

    private void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => ExitApplication();

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

        _pollTimer.Stop();
        _globalHotkeys?.Dispose();
        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
    }
}
