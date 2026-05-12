#if NET8_0_WINDOWS
using System.Windows;

namespace USBVault.Client.Services;

public class AntiCopyService
{
    public void EnableProtection(Window window)
    {
        Security.ClipboardBlocker.Enable(window);
        Security.ScreenshotBlocker.Enable(window);
        Security.PrintBlocker.Enable(window);
    }

    public void DisableProtection(Window window)
    {
        Security.ClipboardBlocker.Disable(window);
        Security.ScreenshotBlocker.Disable(window);
        Security.PrintBlocker.Disable(window);
    }
}
#endif