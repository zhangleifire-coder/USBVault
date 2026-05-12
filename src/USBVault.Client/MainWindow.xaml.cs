using System;

namespace USBVault.Client;

/// <summary>
/// Stub MainWindow for non-Windows builds.
/// On a real Windows machine, this inherits from System.Windows.Window
/// and uses the WPF designer-generated InitializeComponent().
public partial class MainWindow
{
    public MainWindow()
    {
        Console.WriteLine("USBVault.MainWindow initialized (stub build)");
    }
}