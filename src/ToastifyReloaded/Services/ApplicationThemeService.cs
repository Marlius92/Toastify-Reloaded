using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;

namespace ToastifyReloaded.Services;

public static class ApplicationThemeService
{
    public const string Light = "Light";
    public const string Dark = "Dark";
    public const string System = "System";

    public static string Resolve(string? requested)
    {
        if (string.Equals(requested, Dark, StringComparison.OrdinalIgnoreCase))
            return Dark;
        if (string.Equals(requested, System, StringComparison.OrdinalIgnoreCase))
            return IsWindowsAppThemeDark() ? Dark : Light;
        return Light;
    }

    public static void Apply(Window window, string? requested)
    {
        var resolved = Resolve(requested);
        var dark = resolved == Dark;

        var palette = dark
            ? new Palette(
                Background: "#FF202020",
                Panel: "#FF2B2B2B",
                Control: "#FF333333",
                Foreground: "#FFF1F1F1",
                Secondary: "#FFB8B8B8",
                Border: "#FF4A4A4A",
                Accent: "#FF3D6E9E",
                HighlightText: "#FFFFFFFF")
            : new Palette(
                Background: "#FFF0F0F0",
                Panel: "#FFFFFFFF",
                Control: "#FFF8F8F8",
                Foreground: "#FF111111",
                Secondary: "#FF575757",
                Border: "#FFABADB3",
                Accent: "#FF0078D4",
                HighlightText: "#FFFFFFFF");

        var resources = window.Resources;
        resources["AppBackgroundBrush"] = Brush(palette.Background);
        resources["AppPanelBrush"] = Brush(palette.Panel);
        resources["AppControlBrush"] = Brush(palette.Control);
        resources["AppForegroundBrush"] = Brush(palette.Foreground);
        resources["InfoTextColorBrush"] = Brush(palette.Secondary);
        resources["AppBorderBrush"] = Brush(palette.Border);
        resources["AppAccentBrush"] = Brush(palette.Accent);

        // Override the system brush keys locally so native WPF templates also
        // become readable in Dark mode without replacing the historical control templates.
        resources[System.Windows.SystemColors.WindowBrushKey] = Brush(palette.Panel);
        resources[System.Windows.SystemColors.WindowTextBrushKey] = Brush(palette.Foreground);
        resources[System.Windows.SystemColors.ControlBrushKey] = Brush(palette.Control);
        resources[System.Windows.SystemColors.ControlTextBrushKey] = Brush(palette.Foreground);
        resources[System.Windows.SystemColors.GrayTextBrushKey] = Brush(dark ? "#FF8B8B8B" : "#FF6D6D6D");
        resources[System.Windows.SystemColors.HighlightBrushKey] = Brush(palette.Accent);
        resources[System.Windows.SystemColors.HighlightTextBrushKey] = Brush(palette.HighlightText);
        resources[System.Windows.SystemColors.InactiveSelectionHighlightBrushKey] = Brush(dark ? "#FF3B4C5E" : "#FFD6E9F8");
        resources[System.Windows.SystemColors.InactiveSelectionHighlightTextBrushKey] = Brush(palette.Foreground);

        window.Background = (System.Windows.Media.Brush)resources["AppBackgroundBrush"];
        window.Foreground = (System.Windows.Media.Brush)resources["AppForegroundBrush"];

        var handle = new WindowInteropHelper(window).Handle;
        if (handle != IntPtr.Zero)
            TrySetDarkTitleBar(handle, dark);
    }

    public static bool IsWindowsAppThemeDark()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int i && i == 0;
        }
        catch
        {
            return false;
        }
    }

    private static SolidColorBrush Brush(string hex)
    {
        var brush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex)!);
        brush.Freeze();
        return brush;
    }

    private static void TrySetDarkTitleBar(IntPtr hwnd, bool dark)
    {
        try
        {
            var enabled = dark ? 1 : 0;
            // Windows 10 20H1+ uses 20. Older Windows 10 builds used 19.
            if (DwmSetWindowAttribute(hwnd, 20, ref enabled, sizeof(int)) != 0)
                _ = DwmSetWindowAttribute(hwnd, 19, ref enabled, sizeof(int));
        }
        catch
        {
            // Theme support is cosmetic. Never block application startup.
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int dwAttribute,
        ref int pvAttribute,
        int cbAttribute);

    private sealed record Palette(
        string Background,
        string Panel,
        string Control,
        string Foreground,
        string Secondary,
        string Border,
        string Accent,
        string HighlightText);
}
