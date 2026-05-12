#if NET8_0_WINDOWS
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace USBVault.Client.Security;

public static class ScreenshotBlocker
{
    private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;
    private const uint WDA_NONE = 0x00000000;

    [DllImport("user32.dll")]
    private static extern bool SetWindowDisplayAffinity(IntPtr hwnd, uint affinity);

    public static void Enable(Window window)
    {
        var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
        SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE);
    }

    public static void Disable(Window window)
    {
        var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
        SetWindowDisplayAffinity(hwnd, WDA_NONE);
    }
}
#endif