#if NET8_0_WINDOWS
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace USBVault.Client.Security;

public static class ClipboardBlocker
{
    private const int WM_CLIPBOARDUPDATE = 0x031D;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    private static HwndSource? _hwndSource;

    public static void Enable(Window window)
    {
        var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
        AddClipboardFormatListener(hwnd);
        _hwndSource = HwndSource.FromHwnd(hwnd);
        _hwndSource?.AddHook(WndProc);
    }

    public static void Disable(Window window)
    {
        var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
        RemoveClipboardFormatListener(hwnd);
        _hwndSource?.RemoveHook(WndProc);
        _hwndSource = null;
    }

    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_CLIPBOARDUPDATE)
        {
            try
            {
                Clipboard.Clear();
            }
            catch
            {
                // Clipboard access can throw if another process holds it.
                // Silently ignore.
            }
        }
        return IntPtr.Zero;
    }
}
#endif