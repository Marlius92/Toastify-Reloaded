using System.Windows.Input;
using System.Windows.Interop;
using ToastifyModern.Models;
using ToastifyModern.Native;

namespace ToastifyModern.Services;

public sealed class GlobalHotkeyService : IDisposable
{
    private readonly IntPtr _windowHandle;
    private readonly HwndSource _source;
    private readonly Dictionary<int, HotkeyAction> _registrations = new();
    private bool _disposed;

    public event EventHandler<HotkeyAction>? HotkeyPressed;

    public GlobalHotkeyService(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
        _source = HwndSource.FromHwnd(windowHandle)
                  ?? throw new InvalidOperationException("Impossibile collegarsi alla finestra Win32.");
        _source.AddHook(WndProc);
    }

    public IReadOnlyList<string> RegisterAll(IEnumerable<HotkeyBinding> bindings)
    {
        UnregisterAll();
        var errors = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var id = 1000;

        foreach (var binding in bindings)
        {
            if (!seen.Add(binding.Shortcut.Trim()))
            {
                errors.Add($"{binding.ActionLabel}: scorciatoia duplicata ({binding.Shortcut}).");
                continue;
            }

            if (!TryParse(binding.Shortcut, out var modifiers, out var key, out var error))
            {
                errors.Add($"{binding.ActionLabel}: {error}");
                continue;
            }

            var vk = (uint)KeyInterop.VirtualKeyFromKey(key);
            if (!NativeMethods.RegisterHotKey(_windowHandle, id, modifiers | NativeMethods.MOD_NOREPEAT, vk))
            {
                errors.Add($"{binding.ActionLabel}: {binding.Shortcut} è già usata da Windows o da un altro programma.");
                continue;
            }

            _registrations[id] = binding.Action;
            id++;
        }

        return errors;
    }

    private static bool TryParse(string shortcut, out uint modifiers, out Key key, out string error)
    {
        modifiers = 0;
        key = Key.None;
        error = string.Empty;

        var parts = shortcut.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            error = "usa il formato Ctrl+Alt+T.";
            return false;
        }

        foreach (var part in parts[..^1])
        {
            switch (part.ToLowerInvariant())
            {
                case "ctrl":
                case "control":
                    modifiers |= NativeMethods.MOD_CONTROL;
                    break;
                case "alt":
                    modifiers |= NativeMethods.MOD_ALT;
                    break;
                case "shift":
                    modifiers |= NativeMethods.MOD_SHIFT;
                    break;
                case "win":
                case "windows":
                    modifiers |= NativeMethods.MOD_WIN;
                    break;
                default:
                    error = $"modificatore non riconosciuto: {part}.";
                    return false;
            }
        }

        if (modifiers == 0)
        {
            error = "serve almeno un modificatore (Ctrl, Alt, Shift o Win).";
            return false;
        }

        var keyName = parts[^1];
        if (!Enum.TryParse<Key>(keyName, ignoreCase: true, out key) || key == Key.None)
        {
            error = $"tasto non riconosciuto: {keyName}.";
            return false;
        }

        return true;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && _registrations.TryGetValue(wParam.ToInt32(), out var action))
        {
            HotkeyPressed?.Invoke(this, action);
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void UnregisterAll()
    {
        foreach (var id in _registrations.Keys)
            NativeMethods.UnregisterHotKey(_windowHandle, id);
        _registrations.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        UnregisterAll();
        _source.RemoveHook(WndProc);
        _disposed = true;
    }
}
