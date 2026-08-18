using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace ToastifyReloaded.Mac;

public partial class App : Application
{
    public override void Initialize()
        => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private MainWindow? MainWindow
        => ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow as MainWindow
            : null;

    private void TrayIcon_Clicked(object? sender, EventArgs e)
        => MainWindow?.ShowFromTray();

    private void TrayOpen_Click(object? sender, EventArgs e)
        => MainWindow?.ShowFromTray();

    private async void TrayPlayPause_Click(object? sender, EventArgs e)
    {
        if (MainWindow is { } window)
            await window.TrayPlayPauseAsync();
    }

    private async void TrayNext_Click(object? sender, EventArgs e)
    {
        if (MainWindow is { } window)
            await window.TrayNextAsync();
    }

    private async void TrayPrevious_Click(object? sender, EventArgs e)
    {
        if (MainWindow is { } window)
            await window.TrayPreviousAsync();
    }

    private void TrayExit_Click(object? sender, EventArgs e)
        => MainWindow?.RequestExit();
}
