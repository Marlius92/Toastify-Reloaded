using Tmds.DBus;
using ToastifyReloaded.Linux.Models;

namespace ToastifyReloaded.Linux.Services;

public sealed class XdgGlobalShortcutsService : IDisposable
{
    private const string Service = "org.freedesktop.portal.Desktop";
    private static readonly ObjectPath PortalPath =
        new("/org/freedesktop/portal/desktop");

    private readonly PlayerctlService _playerctl;

    private Connection? _connection;
    private IDisposable? _activatedSubscription;
    private IPortalSession? _session;
    private ObjectPath _sessionPath;
    private bool _hasSession;

    public XdgGlobalShortcutsService(PlayerctlService playerctl)
        => _playerctl = playerctl;

    public bool IsWayland =>
        string.Equals(
            Environment.GetEnvironmentVariable("XDG_SESSION_TYPE"),
            "wayland",
            StringComparison.OrdinalIgnoreCase)
        || !string.IsNullOrWhiteSpace(
            Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));

    public async Task<(bool Success, string Message)> ApplyAsync(
        LinuxSettings settings)
    {
        await StopAsync();

        if (!settings.EnableWaylandPortalHotkeys)
            return (true, "Hotkey Wayland disattivate.");

        if (!IsWayland)
            return (false, "Sessione non Wayland.");

        try
        {
            _connection = new Connection(Address.Session);
            await _connection.ConnectAsync();

            var registry = _connection.CreateProxy<IHostPortalRegistry>(
                Service,
                PortalPath);

            try
            {
                await registry.RegisterAsync(
                    "io.github.Marlius92.ToastifyReloaded",
                    new Dictionary<string, object>());
            }
            catch (DBusException)
            {
                // Newer portals support host registration; older portals may
                // identify the application via the desktop/cgroup metadata.
            }

            var portal = _connection.CreateProxy<IGlobalShortcutsPortal>(
                Service,
                PortalPath);

            var createToken = Token("create");
            var sessionToken = Token("session");

            var createHandle = await portal.CreateSessionAsync(
                new Dictionary<string, object>
                {
                    ["handle_token"] = createToken,
                    ["session_handle_token"] = sessionToken
                });

            var createResponse = await WaitForResponseAsync(createHandle);
            if (createResponse.Response != 0)
                return (false, $"Portal CreateSession rifiutato ({createResponse.Response}).");

            if (!TryGetSessionPath(createResponse.Results, out _sessionPath))
                return (false, "Il portale non ha restituito session_handle.");

            _hasSession = true;
            _session = _connection.CreateProxy<IPortalSession>(
                Service,
                _sessionPath);

            _activatedSubscription = await portal.WatchActivatedAsync(
                args => _ = HandleActivatedAsync(args.ShortcutId));

            var shortcuts = BuildShortcuts(settings);
            var bindHandle = await portal.BindShortcutsAsync(
                _sessionPath,
                shortcuts,
                string.Empty,
                new Dictionary<string, object>
                {
                    ["handle_token"] = Token("bind")
                });

            var bindResponse = await WaitForResponseAsync(bindHandle);
            if (bindResponse.Response != 0)
            {
                await StopAsync();
                return (false, $"Portal BindShortcuts rifiutato ({bindResponse.Response}).");
            }

            var count = ExtractBoundCount(bindResponse.Results);
            return (
                true,
                count > 0
                    ? $"Wayland: {count} hotkey registrate tramite XDG Global Shortcuts."
                    : "Wayland: sessione XDG Global Shortcuts attiva.");
        }
        catch (Exception ex)
        {
            await StopAsync();
            return (
                false,
                $"XDG Global Shortcuts non disponibile: {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        _activatedSubscription?.Dispose();
        _activatedSubscription = null;

        if (_hasSession && _session is not null)
        {
            try
            {
                await _session.CloseAsync();
            }
            catch
            {
                // Best effort during shutdown/reconfiguration.
            }
        }

        _session = null;
        _hasSession = false;

        _connection?.Dispose();
        _connection = null;
    }

    public void Dispose()
    {
        try
        {
            StopAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // Best effort.
        }
    }

    private async Task HandleActivatedAsync(string shortcutId)
    {
        switch (shortcutId)
        {
            case "play-pause":
                await _playerctl.PlayPauseAsync();
                break;
            case "next":
                await _playerctl.NextAsync();
                break;
            case "previous":
                await _playerctl.PreviousAsync();
                break;
            case "volume-up":
                await _playerctl.VolumeUpAsync();
                break;
            case "volume-down":
                await _playerctl.VolumeDownAsync();
                break;
            case "mute":
                await _playerctl.ToggleMuteAsync();
                break;
            case "seek-forward":
                await _playerctl.SeekForwardAsync();
                break;
            case "seek-backward":
                await _playerctl.SeekBackwardAsync();
                break;
        }
    }

    private async Task<PortalResponse> WaitForResponseAsync(ObjectPath handle)
    {
        if (_connection is null)
            throw new InvalidOperationException("D-Bus connection is not active.");

        var request = _connection.CreateProxy<IPortalRequest>(
            Service,
            handle);

        var tcs = new TaskCompletionSource<PortalResponse>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        using var subscription = await request.WatchResponseAsync(
            args => tcs.TrySetResult(
                new PortalResponse(args.Response, args.Results)),
            ex => tcs.TrySetException(ex));

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var registration = timeout.Token.Register(
            () => tcs.TrySetException(
                new TimeoutException("Timeout XDG Desktop Portal.")));

        return await tcs.Task;
    }

    private static (string, IDictionary<string, object>)[] BuildShortcuts(
        LinuxSettings settings)
        => new[]
        {
            Shortcut("play-pause", "Play / Pause Spotify", settings.HotkeyPlayPause),
            Shortcut("next", "Brano successivo", settings.HotkeyNext),
            Shortcut("previous", "Brano precedente", settings.HotkeyPrevious),
            Shortcut("volume-up", "Aumenta volume", settings.HotkeyVolumeUp),
            Shortcut("volume-down", "Diminuisci volume", settings.HotkeyVolumeDown),
            Shortcut("mute", "Mute / ripristina volume", settings.HotkeyMute),
            Shortcut("seek-forward", "Avanti 10 secondi", settings.HotkeySeekForward),
            Shortcut("seek-backward", "Indietro 10 secondi", settings.HotkeySeekBackward)
        };

    private static (string, IDictionary<string, object>) Shortcut(
        string id,
        string description,
        string trigger)
    {
        var properties = new Dictionary<string, object>
        {
            ["description"] = description
        };

        var preferred = ToPortalTrigger(trigger);
        if (!string.IsNullOrWhiteSpace(preferred))
            properties["preferred_trigger"] = preferred;

        return (id, properties);
    }

    private static string ToPortalTrigger(string shortcut)
    {
        var parts = shortcut.Split(
            '+',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
            return string.Empty;

        var result = new List<string>();
        foreach (var part in parts)
        {
            result.Add(part.ToLowerInvariant() switch
            {
                "ctrl" or "control" => "CTRL",
                "alt" => "ALT",
                "shift" => "SHIFT",
                "super" or "win" or "meta" => "META",
                "space" => "space",
                "left" => "Left",
                "right" => "Right",
                "up" => "Up",
                "down" => "Down",
                _ => part.Length == 1
                    ? part.ToLowerInvariant()
                    : part
            });
        }

        return string.Join("+", result);
    }

    private static bool TryGetSessionPath(
        IDictionary<string, object> results,
        out ObjectPath path)
    {
        path = default;

        if (!results.TryGetValue("session_handle", out var value))
            return false;

        if (value is ObjectPath objectPath)
        {
            path = objectPath;
            return true;
        }

        if (value is string text && !string.IsNullOrWhiteSpace(text))
        {
            path = new ObjectPath(text);
            return true;
        }

        return false;
    }

    private static int ExtractBoundCount(IDictionary<string, object> results)
    {
        if (!results.TryGetValue("shortcuts", out var value) || value is null)
            return 0;

        if (value is Array array)
            return array.Length;

        return 0;
    }

    private static string Token(string prefix)
        => $"{prefix}_{Guid.NewGuid():N}";

    private readonly record struct PortalResponse(
        uint Response,
        IDictionary<string, object> Results);

    [DBusInterface("org.freedesktop.host.portal.Registry")]
    private interface IHostPortalRegistry : IDBusObject
    {
        Task RegisterAsync(
            string appId,
            IDictionary<string, object> options);
    }

    [DBusInterface("org.freedesktop.portal.GlobalShortcuts")]
    private interface IGlobalShortcutsPortal : IDBusObject
    {
        Task<ObjectPath> CreateSessionAsync(
            IDictionary<string, object> options);

        Task<ObjectPath> BindShortcutsAsync(
            ObjectPath sessionHandle,
            (string, IDictionary<string, object>)[] shortcuts,
            string parentWindow,
            IDictionary<string, object> options);

        Task<IDisposable> WatchActivatedAsync(
            Action<(ObjectPath SessionHandle,
                    string ShortcutId,
                    ulong Timestamp,
                    IDictionary<string, object> Options)> handler,
            Action<Exception>? onError = null);
    }

    [DBusInterface("org.freedesktop.portal.Request")]
    private interface IPortalRequest : IDBusObject
    {
        Task<IDisposable> WatchResponseAsync(
            Action<(uint Response,
                    IDictionary<string, object> Results)> handler,
            Action<Exception>? onError = null);
    }

    [DBusInterface("org.freedesktop.portal.Session")]
    private interface IPortalSession : IDBusObject
    {
        Task CloseAsync();
    }
}
