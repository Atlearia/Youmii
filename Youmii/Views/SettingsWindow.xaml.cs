using System.Windows;
using System.Windows.Input;
using Youmii.ViewModels;

namespace Youmii.Views;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the ViewModel and subscribes to its close request event.
    /// </summary>
    public void SetViewModel(SettingsViewModel viewModel)
    {
        DataContext = viewModel;
        viewModel.RequestClose += OnViewModelRequestClose;
    }

    private void OnViewModelRequestClose(object? sender, bool saved)
    {
        if (sender is SettingsViewModel vm)
        {
            vm.RequestClose -= OnViewModelRequestClose;
        }
        
        DialogResult = saved;
        Close();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
        {
            vm.RequestClose -= OnViewModelRequestClose;
        }
        
        DialogResult = false;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
        {
            vm.RequestClose -= OnViewModelRequestClose;
        }
        base.OnClosed(e);
    }
}
