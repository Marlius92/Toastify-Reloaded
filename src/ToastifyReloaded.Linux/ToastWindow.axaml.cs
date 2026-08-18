using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using ToastifyReloaded.Linux.Models;
using ToastifyReloaded.Linux.Services;

namespace ToastifyReloaded.Linux;

public partial class ToastWindow : Window
{
    private readonly LinuxTrackInfo _track;
    private readonly LinuxSettings _settings;
    private readonly ArtworkService _artworkService;

    public ToastWindow(
        LinuxTrackInfo track,
        LinuxSettings settings,
        ArtworkService artworkService)
    {
        InitializeComponent();
        _track = track;
        _settings = settings;
        _artworkService = artworkService;
    }

    public async Task ShowToastAsync()
    {
        ApplyTheme();
        ApplyContent();
        ApplyWidth();

        var wantsArtwork = _settings.ImageMode != "None";
        ArtworkImage.IsVisible = wantsArtwork;

        if (wantsArtwork)
        {
            var useAlbum = _settings.ImageMode == "Album cover";
            ArtworkImage.Source = await _artworkService.LoadAsync(
                useAlbum ? _track.ArtworkUrl : string.Empty,
                _settings.IconFallback || !useAlbum);
        }

        ProgressBar.IsVisible = _settings.ShowProgress && _track.DurationSeconds > 0;
        TimeText.IsVisible = _settings.ShowSongTime && _track.DurationSeconds > 0;

        var screen = Screens.Primary ?? Screens.All.First();
        PositionAtEnd(screen, entering: true);

        Opacity = 0;
        Show();

        await AnimateInAsync(screen);

        var started = DateTime.UtcNow;
        var initialPosition = _track.PositionSeconds;

        while ((DateTime.UtcNow - started).TotalMilliseconds < _settings.ToastDisplayMs)
        {
            var elapsed = _track.IsPlaying
                ? (DateTime.UtcNow - started).TotalSeconds
                : 0;

            var current = Math.Min(
                _track.DurationSeconds,
                Math.Max(0, initialPosition + elapsed));

            UpdateTimeline(current);
            await Task.Delay(250);
        }

        await AnimateOutAsync(screen);
        Close();
    }

    private void ApplyContent()
    {
        TitleText.Text = string.IsNullOrWhiteSpace(_track.Title)
            ? "Spotify"
            : _track.Title;

        ArtistText.Text = string.IsNullOrWhiteSpace(_track.Artist)
            ? _track.Album
            : _track.Artist;
    }

    private void ApplyWidth()
    {
        if (!_settings.AutoWidth)
        {
            Width = _settings.MinWidth;
            return;
        }

        var longest = Math.Max(
            _track.Title?.Length ?? 0,
            _track.Artist?.Length ?? 0);

        var artworkSpace = _settings.ImageMode == "None" ? 40 : 108;
        var estimate = artworkSpace + 90 + (longest * 7.2);

        Width = Math.Clamp(
            estimate,
            Math.Max(220, _settings.MinWidth),
            Math.Max(_settings.MinWidth, _settings.MaxWidth));
    }

    private void ApplyTheme()
    {
        var palette = ToastThemePalette.FromName(_settings.ToastTheme);

        RootBorder.Background = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.Parse(palette.Top), 0),
                new GradientStop(Color.Parse(palette.Bottom), 1)
            }
        };

        RootBorder.BorderBrush = new SolidColorBrush(Color.Parse(palette.Border));
        TitleText.Foreground = new SolidColorBrush(Color.Parse(palette.Title));
        ArtistText.Foreground = new SolidColorBrush(Color.Parse(palette.Secondary));
        TimeText.Foreground = new SolidColorBrush(Color.Parse(palette.Secondary));
        ProgressBar.Background = new SolidColorBrush(Color.Parse(palette.ProgressBackground));
        ProgressBar.Foreground = new SolidColorBrush(Color.Parse(palette.ProgressForeground));
    }

    private void UpdateTimeline(double current)
    {
        if (_track.DurationSeconds <= 0)
            return;

        ProgressBar.Value = Math.Clamp(
            current / _track.DurationSeconds,
            0,
            1);

        TimeText.Text = $"{FormatTime(current)} / {FormatTime(_track.DurationSeconds)}";
    }

    private static string FormatTime(double seconds)
    {
        var value = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return value.TotalHours >= 1
            ? value.ToString(@"h\:mm\:ss")
            : value.ToString(@"m\:ss");
    }

    private async Task AnimateInAsync(Screen screen)
    {
        var duration = Math.Max(0, _settings.FadeInMs);
        var steps = Math.Max(1, duration / 16);

        var finalPosition = GetFinalPosition(screen);
        var startPosition = OffsetPosition(
            finalPosition,
            _settings.SlideInDirection,
            _settings.AnimationStyle.Contains("Slide", StringComparison.OrdinalIgnoreCase)
                ? _settings.SlideInDistance
                : 0);

        for (var i = 0; i <= steps; i++)
        {
            var t = (double)i / steps;

            Opacity = _settings.AnimationStyle == "None" ? 1 : t;
            Position = Lerp(startPosition, finalPosition, t);

            if (duration > 0)
                await Task.Delay(16);
        }

        Opacity = 1;
        Position = finalPosition;
    }

    private async Task AnimateOutAsync(Screen screen)
    {
        var duration = Math.Max(0, _settings.FadeOutMs);
        var steps = Math.Max(1, duration / 16);

        var startPosition = GetFinalPosition(screen);
        var endPosition = OffsetPosition(
            startPosition,
            _settings.SlideOutDirection,
            _settings.AnimationStyle.Contains("Slide", StringComparison.OrdinalIgnoreCase)
                ? _settings.SlideOutDistance
                : 0);

        for (var i = 0; i <= steps; i++)
        {
            var t = (double)i / steps;

            Opacity = _settings.AnimationStyle == "None" ? 1 : 1 - t;
            Position = Lerp(startPosition, endPosition, t);

            if (duration > 0)
                await Task.Delay(16);
        }
    }

    private void PositionAtEnd(Screen screen, bool entering)
        => Position = entering
            ? OffsetPosition(
                GetFinalPosition(screen),
                _settings.SlideInDirection,
                _settings.SlideInDistance)
            : GetFinalPosition(screen);

    private PixelPoint GetFinalPosition(Screen screen)
    {
        var area = screen.WorkingArea;
        var scale = Math.Max(1, screen.Scaling);
        var pixelWidth = (int)Math.Round(Width * scale);
        var pixelHeight = (int)Math.Round(Height * scale);

        return new PixelPoint(
            area.Right - pixelWidth - 18,
            area.Bottom - pixelHeight - 18);
    }

    private static PixelPoint OffsetPosition(
        PixelPoint point,
        string direction,
        int distance)
        => direction switch
        {
            "Down" => new PixelPoint(point.X, point.Y + distance),
            "Left" => new PixelPoint(point.X - distance, point.Y),
            "Right" => new PixelPoint(point.X + distance, point.Y),
            _ => new PixelPoint(point.X, point.Y - distance)
        };

    private static PixelPoint Lerp(PixelPoint a, PixelPoint b, double t)
        => new(
            (int)Math.Round(a.X + ((b.X - a.X) * t)),
            (int)Math.Round(a.Y + ((b.Y - a.Y) * t)));
}
