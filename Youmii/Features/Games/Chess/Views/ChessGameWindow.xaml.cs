using System.Windows;
using System.Windows.Input;
using Youmii.Features.Games.Chess.ViewModels;

namespace Youmii.Features.Games.Chess.Views;

/// <summary>
/// Code-behind for the Chess game window.
/// </summary>
public partial class ChessGameWindow : Window
{
    public ChessGameWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the ViewModel for the window.
    /// </summary>
    public void SetViewModel(ChessGameViewModel viewModel)
    {
        DataContext = viewModel;
        viewModel.RequestClose += OnRequestClose;
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
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
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is ChessGameViewModel viewModel)
        {
            viewModel.RequestClose -= OnRequestClose;
        }
        base.OnClosed(e);
    }
}
