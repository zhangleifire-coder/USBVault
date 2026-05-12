#if NET8_0_WINDOWS
using System;
using System.Windows;
using System.Windows.Input;

namespace USBVault.Client.Security;

public static class PrintBlocker
{
    private static void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.PrintScreen)
        {
            try
            {
                Clipboard.Clear();
            }
            catch
            {
                // Silently ignore clipboard errors.
            }
            e.Handled = true;
        }
    }

    public static void Enable(Window window)
    {
        window.PreviewKeyDown += Window_PreviewKeyDown;
    }

    public static void Disable(Window window)
    {
        window.PreviewKeyDown -= Window_PreviewKeyDown;
    }
}
#endif