using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
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
    private bool _fadeOutStarted;

    public ToastWindow(TrackInfo track, AppSettings settings)
    {
        _settings = settings;
        InitializeComponent();

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

        ApplyArtwork(track);
        ApplyWidth();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Math.Clamp(settings.ToastDurationMs, 500, 30000)) };
        _timer.Tick += (_, _) =>
        {
            _timer.Stop();
            StartFadeOut();
        };

        Loaded += (_, _) => PositionToast();
        Closed += (_, _) => _timer.Stop();
    }

    public void ShowTimed()
    {
        var fadeInMs = Math.Clamp(_settings.ToastFadeInMs, 0, 5000);
        Opacity = fadeInMs > 0 ? 0 : 1;
        Show();

        if (fadeInMs == 0)
        {
            StartDisplayTimer();
            return;
        }

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(fadeInMs))
        {
            FillBehavior = FillBehavior.Stop
        };
        fadeIn.Completed += (_, _) =>
        {
            Opacity = 1;
            BeginAnimation(OpacityProperty, null);
            StartDisplayTimer();
        };
        BeginAnimation(OpacityProperty, fadeIn);
    }

    private void StartDisplayTimer()
    {
        _timer.Interval = TimeSpan.FromMilliseconds(Math.Clamp(_settings.ToastDurationMs, 500, 30000));
        _timer.Start();
    }

    private void StartFadeOut()
    {
        if (_fadeOutStarted)
            return;

        _fadeOutStarted = true;
        var fadeOutMs = Math.Clamp(_settings.ToastFadeOutMs, 0, 5000);
        if (fadeOutMs == 0)
        {
            Close();
            return;
        }

        var fadeOut = new DoubleAnimation(Opacity, 0, TimeSpan.FromMilliseconds(fadeOutMs))
        {
            FillBehavior = FillBehavior.Stop
        };
        fadeOut.Completed += (_, _) => Close();
        BeginAnimation(OpacityProperty, fadeOut);
    }

    private void ApplyArtwork(TrackInfo track)
    {
        var mode = _settings.ToastImageMode?.Trim() ?? "AlbumCover";

        if (mode.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            HideArtwork();
            return;
        }

        if (mode.Equals("ToastifyIcon", StringComparison.OrdinalIgnoreCase))
        {
            // The historical Toastify logo is already loaded by the XAML resource.
            return;
        }

        // AlbumCover is the default. Use the Windows media-session thumbnail supplied by Spotify.
        if (track.ArtworkBytes is { Length: > 0 } && TryLoadArtwork(track.ArtworkBytes))
            return;

        if (!_settings.ToastImageFallbackToIcon)
            HideArtwork();
    }

    private bool TryLoadArtwork(byte[] bytes)
    {
        try
        {
            using var stream = new MemoryStream(bytes, writable: false);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            AlbumArt.Source = bitmap;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void HideArtwork()
    {
        AlbumArt.Visibility = Visibility.Collapsed;
        ArtworkColumn.Width = new GridLength(0);
        ToastContentGrid.Margin = new Thickness(10, 15, 8, 4);
    }

    private void ApplyWidth()
    {
        if (!_settings.ToastAutoWidth)
        {
            Width = Math.Max(150, _settings.ToastWidth);
            return;
        }

        var minimum = Math.Max(150, _settings.ToastMinWidth);
        var maximum = Math.Max(minimum, _settings.ToastMaxWidth);

        var textWidth = Math.Max(
            MeasureTextWidth(Title1),
            MeasureTextWidth(Title2));

        var artworkSpace = AlbumArt.Visibility == Visibility.Visible
            ? ArtworkColumn.Width.Value
            : 0;

        var horizontalContentMargin = ToastContentGrid.Margin.Left + ToastContentGrid.Margin.Right;
        var borderSpace = Math.Max(0, ToastBorder.BorderThickness.Left) * 2;
        const double trailingSafetyPadding = 14;

        var desired = artworkSpace + horizontalContentMargin + textWidth + trailingSafetyPadding + borderSpace;
        Width = Math.Clamp(Math.Ceiling(desired), minimum, maximum);
    }

    private double MeasureTextWidth(TextBlock textBlock)
    {
        if (string.IsNullOrEmpty(textBlock.Text))
            return 0;

        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        var typeface = new Typeface(
            textBlock.FontFamily,
            textBlock.FontStyle,
            textBlock.FontWeight,
            textBlock.FontStretch);

        var formatted = new FormattedText(
            textBlock.Text,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            typeface,
            textBlock.FontSize,
            Brushes.White,
            dpi);

        return formatted.WidthIncludingTrailingWhitespace;
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
