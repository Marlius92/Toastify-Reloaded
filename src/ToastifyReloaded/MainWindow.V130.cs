using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ToastifyReloaded.Models;
using ToastifyReloaded.Services;
using Forms = System.Windows.Forms;

namespace ToastifyReloaded;

public partial class MainWindow
{
    private bool _loadingSettings;

    private void ApplyApplicationTheme()
    {
        ApplicationThemeService.Apply(this, _settings.ApplicationTheme);
        ApplyTrayTheme();
    }

    private void ApplyTrayTheme()
    {
        var menu = _trayIcon?.ContextMenuStrip;
        if (menu is null)
            return;

        var dark = ApplicationThemeService.Resolve(_settings.ApplicationTheme) == ApplicationThemeService.Dark;
        menu.BackColor = dark ? System.Drawing.Color.FromArgb(43, 43, 43) : System.Drawing.SystemColors.Control;
        menu.ForeColor = dark ? System.Drawing.Color.FromArgb(241, 241, 241) : System.Drawing.SystemColors.ControlText;
        foreach (Forms.ToolStripItem item in menu.Items)
        {
            item.BackColor = menu.BackColor;
            item.ForeColor = menu.ForeColor;
        }
    }

    private void MainWindow_Activated(object? sender, EventArgs e)
    {
        if (_settings.ApplicationTheme.Equals(ApplicationThemeService.System, StringComparison.OrdinalIgnoreCase))
            ApplyApplicationTheme();
    }

    private void ApplicationTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingSettings || !IsLoaded)
            return;

        _settings.ApplicationTheme = ApplicationThemeComboBox.SelectedIndex switch
        {
            1 => ApplicationThemeService.Dark,
            2 => ApplicationThemeService.System,
            _ => ApplicationThemeService.Light
        };
        ApplyApplicationTheme();
    }

    private void ApplicationLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingSettings || !IsLoaded)
            return;

        _settings.ApplicationLanguage = ApplicationLanguageComboBox.SelectedIndex == 1
            ? LocalizationService.Italian
            : LocalizationService.English;
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        string L(string key, string english) => LocalizationService.Get(_settings.ApplicationLanguage, key, english);

        General.Header = L("General", "General");
        Hotkeys.Header = L("Hotkeys", "Hotkeys");
        TabToast.Header = L("Toast", "Toast");
        TabAdvanced.Header = L("Advanced", "Advanced");
        TabReloaded.Header = L("Reloaded", "Reloaded");

        ToastGeneralTab.Header = L("General", "General");
        ToastColorsTab.Header = L("ColorsFont", "Colors & Font");
        ToastThemesTab.Header = L("Themes", "Themes");
        ToastAnimationsTab.Header = L("Animations", "Animations");
        ToastPositionTab.Header = L("Position", "Position");

        BtnSave.Content = L("Save", "Save");
        BtnDefault.Content = L("Default", "Default");
        AppearanceGroupBox.Header = L("Appearance", "Appearance");
        ApplicationThemeLabel.Text = L("ApplicationTheme", "Application theme:");
        ApplicationLanguageLabel.Text = L("Language", "Interface language:");

        if (ApplicationThemeComboBox.Items.Count >= 3)
        {
            ((ComboBoxItem)ApplicationThemeComboBox.Items[0]).Content = L("ThemeLight", "Light");
            ((ComboBoxItem)ApplicationThemeComboBox.Items[1]).Content = L("ThemeDark", "Dark");
            ((ComboBoxItem)ApplicationThemeComboBox.Items[2]).Content = L("ThemeSystem", "Follow Windows");
        }

        SettingsBackupGroupBox.Header = L("SettingsBackup", "Import / export settings");
        DiagnosticsGroupBox.Header = L("Diagnostics", "Diagnostics & compatibility reporting");
        IntegrationsGroupBox.Header = L("Integrations", "Advanced integrations");
        ExportSettingsButton.Content = L("ExportSettings", "Export settings");
        ImportSettingsButton.Content = L("ImportSettings", "Import settings");
        CopyDiagnosticsButton.Content = L("CopyReport", "Copy report");
        ExportDiagnosticsButton.Content = L("ExportReport", "Export report");

        LyricsGroupBox.Header = L("LyricsPlus", "Lyrics Plus");
        CompatibilityGroupBox.Header = L("UpdatesCompatibility", "Updates & Compatibility Guard");
        ToolsGroupBox.Header = L("Tools", "Tools");

        ToastThemePresetLabel.Text = L("Preset", "Theme preset:");
        ApplyToastThemeButton.Content = L("ApplyPreset", "Apply preset");
        AnimationStyleLabel.Text = L("AnimationStyle", "Animation style:");
        AnimationDirectionLabel.Text = L("AnimationDirection", "Slide direction:");
        FadeInLabel.Text = L("FadeIn", "Fade / enter time:");
        FadeOutLabel.Text = L("FadeOut", "Fade / exit time:");
        SlideDistanceLabel.Text = L("SlideDistance", "Slide distance:");
        PositionPresetLabel.Text = L("PositionPreset", "Position preset:");
        MonitorLabel.Text = L("Monitor", "Monitor:");
        ScreenMarginLabel.Text = L("ScreenMargin", "Screen margin:");
    }

    private void PopulateToastThemePresets()
    {
        if (ToastThemePresetComboBox.Items.Count > 0)
            return;

        foreach (var preset in ToastThemePresets.All)
            ToastThemePresetComboBox.Items.Add(preset.Name);
        ToastThemePresetComboBox.Items.Add(ToastThemePresets.CustomName);
    }

    private void ToastThemePreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateToastThemePreview();
    }

    private void ApplyToastTheme_Click(object sender, RoutedEventArgs e)
    {
        var name = ToastThemePresetComboBox.SelectedItem?.ToString();
        var preset = ToastThemePresets.Find(name);
        if (preset is null)
            return;

        ApplyToastThemePresetToUi(preset);
        UpdateToastThemePreview();
    }

    private void ApplyToastThemePresetToUi(ToastThemePreset preset)
    {
        ToastColorTopPicker.SelectedColor = ParseColor(preset.TopColor, System.Windows.Media.Colors.Gray);
        ToastColorBottomPicker.SelectedColor = ParseColor(preset.BottomColor, System.Windows.Media.Colors.Black);
        ToastBorderColorPicker.SelectedColor = ParseColor(preset.BorderColor, System.Windows.Media.Colors.DimGray);
        Title1ColorPicker.SelectedColor = ParseColor(preset.Title1Color, System.Windows.Media.Colors.White);
        Title2ColorPicker.SelectedColor = ParseColor(preset.Title2Color, System.Windows.Media.Colors.WhiteSmoke);
        SongProgressBackgroundColorPicker.SelectedColor = ParseColor(preset.ProgressBackground, System.Windows.Media.Colors.DarkGray);
        SongProgressForegroundColorPicker.SelectedColor = ParseColor(preset.ProgressForeground, System.Windows.Media.Colors.LightGray);
        ColorTopUpDown.Value = 0;
        ColorBottomUpDown.Value = 1;
        BorderThicknessUpDown.Value = preset.BorderThickness;
        BorderTopLeftUpDown.Value = preset.CornerRadius;
        BorderTopRightUpDown.Value = preset.CornerRadius;
        BorderBottomLeftUpDown.Value = preset.CornerRadius;
        BorderBottomRightUpDown.Value = preset.CornerRadius;
        CbToastTitle1DropShadow.IsChecked = preset.Title1Shadow;
        CbToastTitle2DropShadow.IsChecked = preset.Title2Shadow;
    }

    private void UpdateToastThemePreview()
    {
        if (ThemePreviewBorder is null)
            return;

        var name = ToastThemePresetComboBox.SelectedItem?.ToString();
        var preset = ToastThemePresets.Find(name);

        var top = preset?.TopColor ?? ColorToString(ToastColorTopPicker.SelectedColor, "#FF555555");
        var bottom = preset?.BottomColor ?? ColorToString(ToastColorBottomPicker.SelectedColor, "#FF151515");
        var border = preset?.BorderColor ?? ColorToString(ToastBorderColorPicker.SelectedColor, "#FF292929");
        var title1 = preset?.Title1Color ?? ColorToString(Title1ColorPicker.SelectedColor, "#FFFFFFFF");
        var title2 = preset?.Title2Color ?? ColorToString(Title2ColorPicker.SelectedColor, "#FFF0F0F0");
        var progressBg = preset?.ProgressBackground ?? ColorToString(SongProgressBackgroundColorPicker.SelectedColor, "#FF333333");
        var progressFg = preset?.ProgressForeground ?? ColorToString(SongProgressForegroundColorPicker.SelectedColor, "#FFA0A0A0");

        ThemePreviewTop.Color = ParseColor(top, System.Windows.Media.Colors.Gray);
        ThemePreviewBottom.Color = ParseColor(bottom, System.Windows.Media.Colors.Black);
        ThemePreviewBorder.BorderBrush = new SolidColorBrush(ParseColor(border, System.Windows.Media.Colors.DimGray));
        ThemePreviewBorder.BorderThickness = new Thickness(preset?.BorderThickness ?? (BorderThicknessUpDown.Value ?? 1));
        ThemePreviewBorder.CornerRadius = new CornerRadius(preset?.CornerRadius ?? 4);
        ThemePreviewTitle1.Foreground = new SolidColorBrush(ParseColor(title1, System.Windows.Media.Colors.White));
        ThemePreviewTitle2.Foreground = new SolidColorBrush(ParseColor(title2, System.Windows.Media.Colors.WhiteSmoke));
        ThemePreviewProgressBackground.Background = new SolidColorBrush(ParseColor(progressBg, System.Windows.Media.Colors.DarkGray));
        ThemePreviewProgressFill.Background = new SolidColorBrush(ParseColor(progressFg, System.Windows.Media.Colors.LightGray));

        ToastThemeNameText.Text = preset?.Name ?? ToastThemePresets.CustomName;
        ToastThemeDescriptionText.Text = preset?.Description ?? "Current manual Colors & Font configuration.";
    }

    private void ToastAnimationStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateAnimationEnabledState();
    }

    private void UpdateAnimationEnabledState()
    {
        if (ToastAnimationStyleComboBox is null)
            return;

        var style = ToastAnimationStyleComboBox.SelectedIndex;
        var hasSlide = style is 1 or 2;
        var animated = style != 3;
        ToastAnimationDirectionComboBox.IsEnabled = hasSlide;
        ToastSlideDistanceUpDown.IsEnabled = hasSlide;
        FadeInUpDown.IsEnabled = animated;
        FadeOutUpDown.IsEnabled = animated;
    }

    private void PopulateMonitors()
    {
        ToastMonitorComboBox.Items.Clear();
        ToastMonitorComboBox.Items.Add("Primary monitor");

        var screens = Forms.Screen.AllScreens;
        for (var i = 0; i < screens.Length; i++)
        {
            var screen = screens[i];
            var primary = screen.Primary ? " • Primary" : string.Empty;
            ToastMonitorComboBox.Items.Add($"{i + 1}: {screen.DeviceName} — {screen.WorkingArea.Width}×{screen.WorkingArea.Height}{primary}");
        }
    }

    private void ToastPositionPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdatePositionEnabledState();
    }

    private void UpdatePositionEnabledState()
    {
        if (ToastPositionPresetComboBox is null)
            return;

        var custom = ToastPositionPresetComboBox.SelectedIndex == 9;
        PositionLeftUpDown.IsEnabled = custom;
        PositionTopUpDown.IsEnabled = custom;
    }

    private void ExportSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveSettingsFromUi();
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export Toastify Reloaded settings",
                Filter = "Toastify Reloaded settings (*.json)|*.json|JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = $"ToastifyReloaded-settings-{DateTime.Now:yyyyMMdd}.json"
            };
            if (dialog.ShowDialog(this) != true)
                return;

            _settingsService.Export(_settings, dialog.FileName);
            System.Windows.MessageBox.Show("Settings exported successfully.", "Toastify Reloaded", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Unable to export settings", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ImportSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Import Toastify Reloaded settings",
                Filter = "Toastify Reloaded settings (*.json)|*.json|JSON files (*.json)|*.json|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog(this) != true)
                return;

            var imported = _settingsService.Import(dialog.FileName);
            _settings = imported;
            _hotkeys.Clear();
            foreach (var item in _settings.Hotkeys)
                _hotkeys.Add(item);
            LstHotKeys.ItemsSource = _hotkeys;
            LstHotKeys.SelectedIndex = _hotkeys.Count > 0 ? 0 : -1;

            LoadSettingsIntoUi();
            LoadMaintenanceSettingsIntoUi();
            ApplyApplicationTheme();
            ApplyLocalization();
            StartupService.SetEnabled(_settings.StartWithWindows);
            _settingsService.Save(_settings);
            RegisterHotkeys(showSuccess: false);

            System.Windows.MessageBox.Show("Settings imported successfully.", "Toastify Reloaded", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Unable to import settings", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void CopyDiagnostics_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var report = await BuildDiagnosticReportAsync();
            Clipboard.SetText(report);
            System.Windows.MessageBox.Show("Diagnostic report copied to the clipboard.", "Toastify Reloaded", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Unable to create report", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ExportDiagnostics_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var report = await BuildDiagnosticReportAsync();
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export Toastify Reloaded diagnostic report",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = $"ToastifyReloaded-diagnostics-{DateTime.Now:yyyyMMdd-HHmm}.txt"
            };
            if (dialog.ShowDialog(this) != true)
                return;

            await File.WriteAllTextAsync(dialog.FileName, report, Encoding.UTF8);
            System.Windows.MessageBox.Show("Diagnostic report exported successfully.", "Toastify Reloaded", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Unable to export report", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task<string> BuildDiagnosticReportAsync()
    {
        var spotify = await _spotifyInstallation.GetInfoAsync();
        var spicetify = await _compatibility.GetSpicetifyVersionAsync();
        var dpi = VisualTreeHelper.GetDpi(this);
        var screens = Forms.Screen.AllScreens;

        var sb = new StringBuilder();
        sb.AppendLine("Toastify Reloaded diagnostic report");
        sb.AppendLine($"Generated (UTC): {DateTimeOffset.UtcNow:O}");
        sb.AppendLine();
        sb.AppendLine("Application");
        sb.AppendLine($"  Version: {_updateService.CurrentVersion}");
        sb.AppendLine($"  Theme setting: {_settings.ApplicationTheme} ({ApplicationThemeService.Resolve(_settings.ApplicationTheme)})");
        sb.AppendLine($"  Interface language: {_settings.ApplicationLanguage}");
        sb.AppendLine($"  Process architecture: {RuntimeInformation.ProcessArchitecture}");
        sb.AppendLine($"  64-bit process: {Environment.Is64BitProcess}");
        sb.AppendLine();
        sb.AppendLine("Windows");
        sb.AppendLine($"  OS: {RuntimeInformation.OSDescription}");
        sb.AppendLine($"  Runtime: {RuntimeInformation.FrameworkDescription}");
        sb.AppendLine($"  DPI scale: {dpi.DpiScaleX:0.##} × {dpi.DpiScaleY:0.##}");
        sb.AppendLine($"  Connected monitors: {screens.Length}");
        for (var i = 0; i < screens.Length; i++)
        {
            var s = screens[i];
            sb.AppendLine($"    [{i}] {s.DeviceName}: {s.Bounds.Width}×{s.Bounds.Height}, working {s.WorkingArea.Width}×{s.WorkingArea.Height}, primary={s.Primary}");
        }
        sb.AppendLine();
        sb.AppendLine("Spotify / Spicetify");
        sb.AppendLine($"  Spotify detected: {spotify.IsDetected}");
        sb.AppendLine($"  Spotify version: {(spotify.IsDetected ? spotify.Version : "Not detected")}");
        sb.AppendLine($"  Spotify install kind: {(spotify.IsDetected ? spotify.InstallKind : "Not detected")}");
        sb.AppendLine($"  Spicetify version: {(string.IsNullOrWhiteSpace(spicetify) ? "Not detected" : spicetify)}");
        sb.AppendLine($"  Last known compatible Spotify: {_settings.LastKnownSpotifyVersion}");
        sb.AppendLine($"  Last repair attempt version: {_settings.LastAutoRepairAttemptVersion}");
        sb.AppendLine($"  Last repair attempt UTC: {_settings.LastAutoRepairAttemptUtc?.ToString("O") ?? "Never"}");
        sb.AppendLine();
        sb.AppendLine("Toast");
        sb.AppendLine($"  Theme preset: {_settings.ToastThemePreset}");
        sb.AppendLine($"  Animation: {_settings.ToastAnimationStyle} / {_settings.ToastAnimationDirection}");
        sb.AppendLine($"  Position preset: {_settings.ToastPositionPreset}");
        sb.AppendLine($"  Monitor index: {_settings.ToastMonitorIndex}");
        sb.AppendLine($"  Adaptive width: {_settings.ToastAutoWidth} ({_settings.ToastMinWidth:0}–{_settings.ToastMaxWidth:0})");
        sb.AppendLine($"  Artwork mode: {_settings.ToastImageMode}; fallback={_settings.ToastImageFallbackToIcon}");
        sb.AppendLine();
        sb.AppendLine("Maintenance");
        sb.AppendLine($"  Automatic update checks: {_settings.AutoCheckToastifyUpdates}");
        sb.AppendLine($"  Automatic update install: {_settings.AutoInstallToastifyUpdates}");
        sb.AppendLine($"  Automatic Spotify repair: {_settings.AutoRepairAfterSpotifyUpdate}");
        sb.AppendLine($"  Keep Lyrics Plus enabled: {_settings.KeepLyricsPlusEnabled}");
        sb.AppendLine();
        sb.AppendLine("Privacy note: this report intentionally excludes Spotify credentials, song history and the contents of personal files.");
        return sb.ToString();
    }
    private static int AnimationStyleToIndex(string? value) => value switch
    {
        "Slide" => 1,
        "FadeSlide" => 2,
        "None" => 3,
        _ => 0
    };

    private static string AnimationStyleFromIndex(int index) => index switch
    {
        1 => "Slide",
        2 => "FadeSlide",
        3 => "None",
        _ => "Fade"
    };

    private static int AnimationDirectionToIndex(string? value) => value switch
    {
        "Down" => 1,
        "Left" => 2,
        "Right" => 3,
        _ => 0
    };

    private static string AnimationDirectionFromIndex(int index) => index switch
    {
        1 => "Down",
        2 => "Left",
        3 => "Right",
        _ => "Up"
    };

    private static int PositionPresetToIndex(string? value) => value switch
    {
        "TopCenter" => 1,
        "TopRight" => 2,
        "MiddleLeft" => 3,
        "Center" => 4,
        "MiddleRight" => 5,
        "BottomLeft" => 6,
        "BottomCenter" => 7,
        "BottomRight" => 8,
        "Custom" => 9,
        _ => 0
    };

    private static string PositionPresetFromIndex(int index) => index switch
    {
        1 => "TopCenter",
        2 => "TopRight",
        3 => "MiddleLeft",
        4 => "Center",
        5 => "MiddleRight",
        6 => "BottomLeft",
        7 => "BottomCenter",
        8 => "BottomRight",
        9 => "Custom",
        _ => "TopLeft"
    };

}
