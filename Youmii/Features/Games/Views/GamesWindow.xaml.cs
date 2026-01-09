using System.Windows;
using System.Windows.Input;
using Youmii.Features.Games.ViewModels;

namespace Youmii.Features.Games.Views;

/// <summary>
/// Code-behind for the Games selection window.
/// </summary>
public partial class GamesWindow : Window
{
    public GamesWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the ViewModel for the window.
    /// </summary>
    public void SetViewModel(GamesViewModel viewModel)
    {
        DataContext = viewModel;
        viewModel.RequestClose += OnRequestClose;
        viewModel.GameLaunched += OnGameLaunched;
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnGameLaunched(object? sender, string gameId)
    {
        DialogResult = true;
        Close();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is GamesViewModel viewModel)
        {
            viewModel.RequestClose -= OnRequestClose;
            viewModel.GameLaunched -= OnGameLaunched;
        }
        base.OnClosed(e);
    }
}
