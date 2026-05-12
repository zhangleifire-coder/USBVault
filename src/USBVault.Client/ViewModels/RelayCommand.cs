using System;

namespace USBVault.Client.ViewModels;

// RelayCommand stub for non-Windows builds.
// On Windows .NET 8 Desktop, the full RelayCommand is used from the WPF build.
#if !WINDOWS_DESKTOP
public class RelayCommand
{
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null) { }
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object? parameter) => false;
    public void Execute(object? parameter) { }
}
#endif