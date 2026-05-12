using System;

#if WINDOWS_DESKTOP
using System.Windows;
using System.Windows.Input;
using USBVault.Client.Models;
using USBVault.Client.Services;
using USBVault.Client.ViewModels;

namespace USBVault.Client;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private bool _adminLoginVisible = false;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        Loaded += OnLoaded;
        SourceInitialized += InitializeProtection;

        AdminLoginPanel.Visibility = Visibility.Collapsed;
    }

    private void InitializeProtection(object? sender, EventArgs e)
    {
        AntiCopyService.EnableProtection(this);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AuthWhitelist whitelist = LoadWhitelist();
        _viewModel.Initialize(whitelist);
        RefreshUIFromViewModel();
    }

    private AuthWhitelist LoadWhitelist()
    {
        return new AuthWhitelist(new System.Collections.Generic.List<AuthorizedMachine>(), 5);
    }

    private void RefreshUIFromViewModel()
    {
        bool isAuthorized = _viewModel.IsAuthorized;
        string statusText = _viewModel.StatusText;
        string authCountText = _viewModel.AuthCountText;

        StatusBarText.Text = $"授权: {authCountText} — {statusText}";
        StatusBar.Background = isAuthorized
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x19, 0x76, 0xD2))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xD3, 0x2F, 0x2F));

        StatusIcon.Text = isAuthorized ? "✓" : "✗";
        StatusIcon.Foreground = isAuthorized
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x2E, 0x7D, 0x32))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xC6, 0x28, 0x28));

        StatusLabel.Text = statusText;
        AuthCountLabel.Text = $"已授权 {authCountText} 台电脑";
        AdminAuthCountLabel.Text = $"当前授权: {authCountText} 台";
        BtnOpenFile.IsEnabled = isAuthorized;
    }

    private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "打开加密文件",
            Filter = "所有文件 (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            MessageBox.Show($"已选择文件: {dialog.FileName}\n解密功能开发中。",
                "打开文件", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void BtnExit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void BtnAdminLogin_Click(object sender, RoutedEventArgs e)
    {
        _adminLoginVisible = true;
        AdminLoginPanel.Visibility = Visibility.Visible;
        AdminPasswordBox.Focus();
    }

    private void BtnConfirmLogin_Click(object sender, RoutedEventArgs e)
    {
        AttemptAdminLogin();
    }

    private void AdminPasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            AttemptAdminLogin();
        else if (e.Key == Key.Escape)
            CancelAdminLogin();
    }

    private void BtnCancelLogin_Click(object sender, RoutedEventArgs e)
    {
        CancelAdminLogin();
    }

    private void AttemptAdminLogin()
    {
        string password = AdminPasswordBox.Password;
        AdminPasswordBox.Password = string.Empty;

        if (!_viewModel.CanLogin(password))
        {
            MessageBox.Show("密码错误，请重试。", "认证失败",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        EnterAdminMode();
    }

    private void CancelAdminLogin()
    {
        _adminLoginVisible = false;
        AdminLoginPanel.Visibility = Visibility.Collapsed;
        AdminPasswordBox.Password = string.Empty;
    }

    private void EnterAdminMode()
    {
        _adminLoginVisible = false;
        AdminLoginPanel.Visibility = Visibility.Collapsed;

        UserModePanel.Visibility = Visibility.Collapsed;
        AdminModePanel.Visibility = Visibility.Visible;

        MachinesDataGrid.ItemsSource = _viewModel.Machines;
        AdminAuthCountLabel.Text = $"当前授权: {_viewModel.AuthCountText} 台";
        BtnRemoveMachine.IsEnabled = false;
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        AdminModePanel.Visibility = Visibility.Collapsed;
        UserModePanel.Visibility = Visibility.Visible;
        MachinesDataGrid.ItemsSource = null;
        RefreshUIFromViewModel();
    }

    private void BtnAddMachine_Click(object sender, RoutedEventArgs e)
    {
        string fullFingerprint = new FingerprintService().GenerateFingerprint();
        string shortId = new FingerprintService().GetShortId(fullFingerprint);

        if (_viewModel.Machines.Count >= _viewModel.MaxAllowed)
        {
            MessageBox.Show($"已达授权上限（{_viewModel.MaxAllowed}台），无法添加更多电脑。",
                "已达上限", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        foreach (var m in _viewModel.Machines)
        {
            if (string.Equals(m.ShortId, shortId, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("当前电脑已在授权列表中。", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }

        _viewModel.AddMachine(shortId);
        MachinesDataGrid.ItemsSource = null;
        MachinesDataGrid.ItemsSource = _viewModel.Machines;
        AdminAuthCountLabel.Text = $"当前授权: {_viewModel.AuthCountText} 台";

        RefreshUIFromViewModel();
    }

    private void BtnRemoveMachine_Click(object sender, RoutedEventArgs e)
    {
        if (MachinesDataGrid.SelectedItem is not AuthorizedMachine selected)
            return;

        var result = MessageBox.Show(
            $"确定要移除机器 {selected.ShortId} 吗？",
            "确认移除",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        _viewModel.RemoveMachine(selected.ShortId);
        MachinesDataGrid.ItemsSource = null;
        MachinesDataGrid.ItemsSource = _viewModel.Machines;
        BtnRemoveMachine.IsEnabled = false;
        AdminAuthCountLabel.Text = $"当前授权: {_viewModel.AuthCountText} 台";

        RefreshUIFromViewModel();
    }

    private void MachinesDataGrid_SelectionChanged(object sender, RoutedEventArgs e)
    {
        BtnRemoveMachine.IsEnabled = MachinesDataGrid.SelectedItem != null;
    }

    private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("密码修改功能开发中。", "提示",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public AuthWhitelist GetWhitelist() => _viewModel.GetWhitelist();
}
#endif