using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using WpfSystemColors = global::System.Windows.SystemColors;
using WpfBrush = global::System.Windows.Media.Brush;
using WpfSolidColorBrush = global::System.Windows.Media.SolidColorBrush;
using WpfColor = global::System.Windows.Media.Color;
using WpfColorConverter = global::System.Windows.Media.ColorConverter;
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
                Hover: "#FF3A3A3A",
                Pressed: "#FF444444",
                DisabledForeground: "#FF8D8D8D",
                HighlightText: "#FFFFFFFF")
            : new Palette(
                Background: "#FFF0F0F0",
                Panel: "#FFFFFFFF",
                Control: "#FFF8F8F8",
                Foreground: "#FF111111",
                Secondary: "#FF575757",
                Border: "#FFABADB3",
                Accent: "#FF0078D4",
                Hover: "#FFE8F2FB",
                Pressed: "#FFD5E8F7",
                DisabledForeground: "#FF777777",
                HighlightText: "#FFFFFFFF");

        // Apply the palette both locally and at application scope. Some native
        // WPF templates resolve SystemColors from Application resources when they
        // are first materialized, so updating only Window.Resources left white
        // TabControl/ComboBox surfaces behind in Dark mode.
        ApplyPalette(window.Resources, palette, dark);
        if (Application.Current is not null)
            ApplyPalette(Application.Current.Resources, palette, dark);

        window.Tag = resolved;
        window.Background = (WpfBrush)window.Resources["AppBackgroundBrush"];
        window.Foreground = (WpfBrush)window.Resources["AppForegroundBrush"];

        ApplyExplicitSurfaces(window, palette);

        var handle = new WindowInteropHelper(window).Handle;
        if (handle != IntPtr.Zero)
            TrySetDarkTitleBar(handle, dark);
    }


    private static void ApplyPalette(ResourceDictionary resources, Palette palette, bool dark)
    {
        resources["AppBackgroundBrush"] = Brush(palette.Background);
        resources["AppPanelBrush"] = Brush(palette.Panel);
        resources["AppControlBrush"] = Brush(palette.Control);
        resources["AppForegroundBrush"] = Brush(palette.Foreground);
        resources["InfoTextColorBrush"] = Brush(palette.Secondary);
        resources["AppBorderBrush"] = Brush(palette.Border);
        resources["AppAccentBrush"] = Brush(palette.Accent);
        resources["AppHoverBrush"] = Brush(palette.Hover);
        resources["AppPressedBrush"] = Brush(palette.Pressed);
        resources["AppDisabledForegroundBrush"] = Brush(palette.DisabledForeground);

        resources[WpfSystemColors.WindowBrushKey] = Brush(palette.Panel);
        resources[WpfSystemColors.WindowTextBrushKey] = Brush(palette.Foreground);
        resources[WpfSystemColors.ControlBrushKey] = Brush(palette.Control);
        resources[WpfSystemColors.ControlTextBrushKey] = Brush(palette.Foreground);
        resources[WpfSystemColors.GrayTextBrushKey] = Brush(palette.DisabledForeground);
        resources[WpfSystemColors.HighlightBrushKey] = Brush(palette.Accent);
        resources[WpfSystemColors.HighlightTextBrushKey] = Brush(palette.HighlightText);
        resources[WpfSystemColors.InactiveSelectionHighlightBrushKey] = Brush(dark ? "#FF3B4C5E" : "#FFD6E9F8");
        resources[WpfSystemColors.InactiveSelectionHighlightTextBrushKey] = Brush(palette.Foreground);
    }

    private static void ApplyExplicitSurfaces(DependencyObject root, Palette palette)
    {
        var panel = Brush(palette.Panel);
        var control = Brush(palette.Control);
        var foreground = Brush(palette.Foreground);
        var border = Brush(palette.Border);

        void Walk(DependencyObject node)
        {
            switch (node)
            {
                case System.Windows.Controls.TabControl tabControl:
                    tabControl.Background = panel;
                    tabControl.Foreground = foreground;
                    tabControl.BorderBrush = border;
                    break;
                case System.Windows.Controls.GroupBox groupBox:
                    groupBox.Background = panel;
                    groupBox.Foreground = foreground;
                    groupBox.BorderBrush = border;
                    break;
                case System.Windows.Controls.ComboBox comboBox:
                    comboBox.Background = control;
                    comboBox.Foreground = foreground;
                    comboBox.BorderBrush = border;
                    break;
                case System.Windows.Controls.TextBox textBox:
                    textBox.Background = control;
                    textBox.Foreground = foreground;
                    textBox.BorderBrush = border;
                    textBox.CaretBrush = foreground;
                    break;
                case System.Windows.Controls.ListBox listBox:
                    listBox.Background = panel;
                    listBox.Foreground = foreground;
                    listBox.BorderBrush = border;
                    break;
            }

            var count = VisualTreeHelper.GetChildrenCount(node);
            for (var i = 0; i < count; i++)
                Walk(VisualTreeHelper.GetChild(node, i));
        }

        Walk(root);
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

    private static WpfSolidColorBrush Brush(string hex)
    {
        var brush = new WpfSolidColorBrush((WpfColor)WpfColorConverter.ConvertFromString(hex)!);
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
        string Hover,
        string Pressed,
        string DisabledForeground,
        string HighlightText);
}
