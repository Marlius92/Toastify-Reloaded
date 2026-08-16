using System.Runtime.InteropServices;
using ToastifyReloaded.Native;

namespace ToastifyReloaded.Services;

public static class MediaKeyService
{
    public static void VolumeUp() => SendKey(NativeMethods.VK_VOLUME_UP);
    public static void VolumeDown() => SendKey(NativeMethods.VK_VOLUME_DOWN);
    public static void ToggleMute() => SendKey(NativeMethods.VK_VOLUME_MUTE);

    private static void SendKey(ushort virtualKey)
    {
        var inputs = new[]
        {
            new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD,
                U = new NativeMethods.InputUnion
                {
                    ki = new NativeMethods.KEYBDINPUT { wVk = virtualKey }
                }
            },
            new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD,
                U = new NativeMethods.InputUnion
                {
                    ki = new NativeMethods.KEYBDINPUT
                    {
                        wVk = virtualKey,
                        dwFlags = NativeMethods.KEYEVENTF_KEYUP
                    }
                }
            }
        };

        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
            throw new InvalidOperationException("Windows non ha accettato il comando multimediale.");
    }
}
