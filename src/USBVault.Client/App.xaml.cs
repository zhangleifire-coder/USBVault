using System;

namespace USBVault.Client;

/// <summary>
/// Stub App entry point for non-Windows builds.
/// On a real Windows machine, WPF Application class is available via
/// Microsoft.WindowsDesktop.Sdk framework reference and this class
/// inherits from System.Windows.Application instead.
public class App
{
    public static void Main()
    {
        // Entry point - actual WPF startup is in the WPF-enabled version
        Console.WriteLine("USBVault.Client (stub build)");
    }
}