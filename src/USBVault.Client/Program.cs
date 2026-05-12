using System;

namespace USBVault.Client;

// Entry point stub for non-Windows builds (macOS cross-compile).
// On Windows .NET 8 Desktop the XAML-generated App.g.cs provides the real entry point.
public class Program
{
    public static void Main()
    {
        Console.WriteLine("USBVault.Client: Build on Windows .NET 8 Desktop for full WPF UI.");
    }
}