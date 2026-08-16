using System.Windows;
using System.Windows.Threading;
using ToastifyReloaded.Models;

namespace ToastifyReloaded;

public partial class ToastWindow : Window
{
    private readonly DispatcherTimer _timer;

    public ToastWindow(TrackInfo track, int durationMs)
    {
        InitializeComponent();
        TitleText.Text = track.Title;
        ArtistText.Text = track.Artist;
        AlbumText.Text = track.Album;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Math.Max(1000, durationMs)) };
        _timer.Tick += (_, _) =>
        {
            _timer.Stop();
            Close();
        };

        Loaded += (_, _) => PositionBottomRight();
    }

    public void ShowTimed()
    {
        Show();
        _timer.Start();
    }

    private void PositionBottomRight()
    {
        var area = SystemParameters.WorkArea;
        Left = area.Right - ActualWidth - 18;
        Top = area.Bottom - ActualHeight - 18;
    }
}
