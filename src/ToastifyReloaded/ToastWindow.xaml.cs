using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using MediaColor = System.Windows.Media.Color;
using MediaColors = System.Windows.Media.Colors;
using MediaColorConverter = System.Windows.Media.ColorConverter;
using ToastifyReloaded.Models;

namespace ToastifyReloaded;

public partial class ToastWindow : Window
{
    private readonly DispatcherTimer _timer;
    private readonly AppSettings _settings;

    public ToastWindow(TrackInfo track, AppSettings settings)
    {
        _settings = settings;
        InitializeComponent();

        Width = Math.Max(150, settings.ToastWidth);
        Height = Math.Max(50, settings.ToastHeight);

        if (settings.ToastTitlesOrder.Equals("ArtistTrack", StringComparison.OrdinalIgnoreCase))
        {
            Title1.Text = track.Artist;
            Title2.Text = track.Title;
        }
        else
        {
            Title1.Text = track.Title;
            Title2.Text = track.Artist;
        }

        ToastBorder.BorderThickness = new Thickness(Math.Max(0, settings.ToastBorderThickness));
        ToastBorder.CornerRadius = new CornerRadius(
            Math.Max(0, settings.ToastCornerTopLeft),
            Math.Max(0, settings.ToastCornerTopRight),
            Math.Max(0, settings.ToastCornerBottomRight),
            Math.Max(0, settings.ToastCornerBottomLeft));
        ToastBorder.BorderBrush = new SolidColorBrush(ParseColor(settings.ToastBorderColor, MediaColor.FromArgb(255, 41, 41, 41)));
        TopGradientStop.Color = ParseColor(settings.ToastColorTop, MediaColor.FromArgb(255, 85, 85, 85));
        BottomGradientStop.Color = ParseColor(settings.ToastColorBottom, MediaColor.FromArgb(255, 21, 21, 21));
        TopGradientStop.Offset = Math.Clamp(settings.ToastColorTopOffset, 0, 1);
        BottomGradientStop.Offset = Math.Clamp(settings.ToastColorBottomOffset, TopGradientStop.Offset, 1);
        Title1.Foreground = new SolidColorBrush(ParseColor(settings.ToastTitle1Color, MediaColors.White));
        Title2.Foreground = new SolidColorBrush(ParseColor(settings.ToastTitle2Color, MediaColor.FromArgb(255, 240, 240, 240)));
        Title1.FontSize = Math.Clamp(settings.ToastTitle1FontSize, 6, 40);
        Title2.FontSize = Math.Clamp(settings.ToastTitle2FontSize, 6, 40);
        if (settings.ToastTitle1DropShadow)
            Title1.Effect = new DropShadowEffect { ShadowDepth = Math.Clamp(settings.ToastTitle1ShadowDepth, 0, 8), BlurRadius = Math.Clamp(settings.ToastTitle1ShadowBlur, 0, 24), Opacity = 0.8 };
        if (settings.ToastTitle2DropShadow)
            Title2.Effect = new DropShadowEffect { ShadowDepth = Math.Clamp(settings.ToastTitle2ShadowDepth, 0, 8), BlurRadius = Math.Clamp(settings.ToastTitle2ShadowBlur, 0, 24), Opacity = 0.8 };
        SongProgressBarContainer.Background = new SolidColorBrush(ParseColor(settings.SongProgressBarBackgroundColor, MediaColor.FromArgb(255, 51, 51, 51)));
        var progressBrush = new SolidColorBrush(ParseColor(settings.SongProgressBarForegroundColor, MediaColor.FromArgb(255, 160, 160, 160)));
        SongProgressBarLine.Background = progressBrush;
        SongProgressBarLineEllipse.Fill = progressBrush;
        SongProgressBar.Visibility = settings.ShowSongProgressBar ? Visibility.Visible : Visibility.Collapsed;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Math.Clamp(settings.ToastDurationMs, 500, 30000)) };
        _timer.Tick += (_, _) =>
        {
            _timer.Stop();
            Close();
        };

        Loaded += (_, _) => PositionToast();
    }

    public void ShowTimed()
    {
        Show();
        _timer.Start();
    }

    private void PositionToast()
    {
        var area = SystemParameters.WorkArea;
        var left = _settings.PositionLeft;
        var top = _settings.PositionTop;

        if (left < area.Left || left + ActualWidth > area.Right)
            left = area.Right - ActualWidth;
        if (top < area.Top || top + ActualHeight > area.Bottom)
            top = area.Bottom - ActualHeight;

        Left = left;
        Top = top;
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); }
            catch { }
        }
    }

    private static MediaColor ParseColor(string value, MediaColor fallback)
    {
        try { return (MediaColor)MediaColorConverter.ConvertFromString(value)!; }
        catch { return fallback; }
    }
}
