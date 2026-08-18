using SharpHook;
using SharpHook.Data;
using SharpHook.Native;
using SharpHook.Providers;
using ToastifyReloaded.Mac.Models;

namespace ToastifyReloaded.Mac.Services;

public sealed class MacGlobalHotkeyService : IDisposable
{
    private readonly SpotifyAppleScriptService _spotify;
    private readonly object _sync = new();
    private readonly HashSet<KeyCode> _pressed = new();
    private readonly Dictionary<Shortcut, Func<Task>> _bindings = new();

    private SimpleGlobalHook? _hook;
    private Task? _hookTask;

    public string Status { get; private set; } = "Hotkey globali non inizializzate.";
    public bool PermissionGranted { get; private set; }
    public int BindingCount => _bindings.Count;

    public MacGlobalHotkeyService(SpotifyAppleScriptService spotify)
        => _spotify = spotify;

    public Task<(bool Success, string Message)> ApplyAsync(MacSettings settings)
    {
        DisposeHook();
        _bindings.Clear();

        if (!settings.EnableGlobalHotkeys)
        {
            Status = "Hotkey globali disattivate.";
            return Task.FromResult((true, Status));
        }

        if (!OperatingSystem.IsMacOS())
        {
            Status = "Backend hotkey macOS disponibile solo su macOS.";
            return Task.FromResult((false, Status));
        }

        PermissionGranted = UioHook.IsAxApiEnabled(promptUserIfDisabled: true);
        if (!PermissionGranted)
        {
            Status = "Concedi a Toastify Reloaded l'accesso in Impostazioni di Sistema → Privacy e Sicurezza → Accessibilità, poi premi Salva.";
            return Task.FromResult((false, Status));
        }

        AddBinding(settings.HotkeyPlayPause, _spotify.PlayPauseAsync);
        AddBinding(settings.HotkeyNext, _spotify.NextAsync);
        AddBinding(settings.HotkeyPrevious, _spotify.PreviousAsync);
        AddBinding(settings.HotkeyVolumeUp, _spotify.VolumeUpAsync);
        AddBinding(settings.HotkeyVolumeDown, _spotify.VolumeDownAsync);
        AddBinding(settings.HotkeyMute, _spotify.ToggleMuteAsync);
        AddBinding(settings.HotkeySeekForward, _spotify.SeekForwardAsync);
        AddBinding(settings.HotkeySeekBackward, _spotify.SeekBackwardAsync);

        UioHookProvider.Instance.KeyTypedEnabled = false;

        _hook = new SimpleGlobalHook(
            GlobalHookType.Keyboard,
            runAsyncOnBackgroundThread: true);

        _hook.KeyPressed += Hook_KeyPressed;
        _hook.KeyReleased += Hook_KeyReleased;

        try
        {
            _hookTask = _hook.RunAsync();
            Status = $"Hotkey globali macOS attive ({_bindings.Count}).";
            return Task.FromResult((true, Status));
        }
        catch (Exception ex)
        {
            DisposeHook();
            Status = $"Impossibile avviare le hotkey globali: {ex.Message}";
            return Task.FromResult((false, Status));
        }
    }

    internal static Shortcut? ParseShortcut(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var parts = text
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        EventMask modifiers = EventMask.None;
        string? keyText = null;

        foreach (var raw in parts)
        {
            var token = raw.Trim();

            if (token.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
                token.Equals("Control", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= EventMask.Ctrl;
            }
            else if (token.Equals("Alt", StringComparison.OrdinalIgnoreCase) ||
                     token.Equals("Option", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= EventMask.Alt;
            }
            else if (token.Equals("Shift", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= EventMask.Shift;
            }
            else if (token.Equals("Meta", StringComparison.OrdinalIgnoreCase) ||
                     token.Equals("Cmd", StringComparison.OrdinalIgnoreCase) ||
                     token.Equals("Command", StringComparison.OrdinalIgnoreCase) ||
                     token.Equals("Super", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= EventMask.Meta;
            }
            else
            {
                if (keyText is not null)
                    return null;

                keyText = token;
            }
        }

        if (string.IsNullOrWhiteSpace(keyText) || !TryParseKeyCode(keyText, out var keyCode))
            return null;

        return new Shortcut(keyCode, modifiers);
    }

    private static bool TryParseKeyCode(string token, out KeyCode keyCode)
    {
        var normalized = token.Trim();

        var aliases = new Dictionary<string, KeyCode>(StringComparer.OrdinalIgnoreCase)
        {
            ["Space"] = KeyCode.VcSpace,
            ["Left"] = KeyCode.VcLeft,
            ["Right"] = KeyCode.VcRight,
            ["Up"] = KeyCode.VcUp,
            ["Down"] = KeyCode.VcDown,
            ["Enter"] = KeyCode.VcEnter,
            ["Return"] = KeyCode.VcEnter,
            ["Escape"] = KeyCode.VcEscape,
            ["Esc"] = KeyCode.VcEscape,
            ["Tab"] = KeyCode.VcTab,
            ["Home"] = KeyCode.VcHome,
            ["End"] = KeyCode.VcEnd,
            ["PageUp"] = KeyCode.VcPageUp,
            ["PageDown"] = KeyCode.VcPageDown,
            ["MediaPlay"] = KeyCode.VcMediaPlay,
            ["MediaNext"] = KeyCode.VcMediaNext,
            ["MediaPrevious"] = KeyCode.VcMediaPrevious,
            ["VolumeUp"] = KeyCode.VcVolumeUp,
            ["VolumeDown"] = KeyCode.VcVolumeDown,
            ["VolumeMute"] = KeyCode.VcVolumeMute
        };

        if (aliases.TryGetValue(normalized, out keyCode))
            return true;

        if (normalized.Length == 1 && char.IsLetterOrDigit(normalized[0]))
        {
            var enumName = "Vc" + char.ToUpperInvariant(normalized[0]);
            return Enum.TryParse(enumName, ignoreCase: true, out keyCode);
        }

        if (normalized.Length is 2 or 3 &&
            normalized.StartsWith('F') &&
            int.TryParse(normalized[1..], out var f) &&
            f is >= 1 and <= 20)
        {
            return Enum.TryParse("VcF" + f, ignoreCase: true, out keyCode);
        }

        return Enum.TryParse(
            normalized.StartsWith("Vc", StringComparison.OrdinalIgnoreCase)
                ? normalized
                : "Vc" + normalized,
            ignoreCase: true,
            out keyCode);
    }

    private void AddBinding(string text, Func<Task> action)
    {
        var shortcut = ParseShortcut(text);
        if (shortcut is not null)
            _bindings[shortcut.Value] = action;
    }

    private void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        var key = e.Data.KeyCode;

        lock (_sync)
        {
            if (!_pressed.Add(key))
                return;
        }

        var modifiers = NormalizeModifiers(e.RawEvent.Mask);
        if (_bindings.TryGetValue(new Shortcut(key, modifiers), out var action))
            _ = Task.Run(action);
    }

    private void Hook_KeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        lock (_sync)
            _pressed.Remove(e.Data.KeyCode);
    }

    private void DisposeHook()
    {
        if (_hook is null)
            return;

        try
        {
            _hook.KeyPressed -= Hook_KeyPressed;
            _hook.KeyReleased -= Hook_KeyReleased;
            _hook.Stop();
            _hook.Dispose();
        }
        catch
        {
        }
        finally
        {
            _hook = null;
            _hookTask = null;
            lock (_sync)
                _pressed.Clear();
        }
    }

    public void Dispose() => DisposeHook();

    internal static EventMask NormalizeModifiers(EventMask raw)
    {
        EventMask normalized = EventMask.None;

        if ((raw & EventMask.Ctrl) != 0)
            normalized |= EventMask.Ctrl;
        if ((raw & EventMask.Alt) != 0)
            normalized |= EventMask.Alt;
        if ((raw & EventMask.Shift) != 0)
            normalized |= EventMask.Shift;
        if ((raw & EventMask.Meta) != 0)
            normalized |= EventMask.Meta;

        return normalized;
    }

    public readonly record struct Shortcut(KeyCode KeyCode, EventMask Modifiers);
}
