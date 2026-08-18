using Avalonia;
using Avalonia.Controls;
using Avalonia.Fonts.Inter;
using ToastifyReloaded.Linux.Services;

namespace ToastifyReloaded.Linux;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Contains(
                "--self-test",
                StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode =
                LinuxSelfTestService
                    .RunAsync()
                    .GetAwaiter()
                    .GetResult();

            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(
            args,
            ShutdownMode.OnExplicitShutdown);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

        var waylandDisplay =
            Environment.GetEnvironmentVariable(
                "WAYLAND_DISPLAY");

        var disableNativeWayland =
            string.Equals(
                Environment.GetEnvironmentVariable(
                    "TOASTIFY_DISABLE_NATIVE_WAYLAND"),
                "1",
                StringComparison.Ordinal);

        if (OperatingSystem.IsLinux() &&
            !disableNativeWayland &&
            !string.IsNullOrWhiteSpace(waylandDisplay))
        {
            builder = builder.UseWayland();
        }

        return builder;
    }
}
