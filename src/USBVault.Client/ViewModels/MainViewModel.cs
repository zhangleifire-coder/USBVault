using System;

namespace USBVault.Client.ViewModels;

// ViewModel stubs for non-Windows builds.
// On Windows .NET 8 Desktop, the full MainViewModel is used from the WPF build.
#if !WINDOWS_DESKTOP
public class MainViewModel
{
    public bool IsAuthorized => false;
    public string StatusText => "macOS stub";
    public string AuthCountText => "0/5";
}
#endif