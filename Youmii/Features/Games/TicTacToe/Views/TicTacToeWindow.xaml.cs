using System.Windows;
using System.Windows.Input;
using Youmii.Features.Games.TicTacToe.ViewModels;

namespace Youmii.Features.Games.TicTacToe.Views;

/// <summary>
/// Code-behind for Tic Tac Toe window.
/// </summary>
public partial class TicTacToeWindow : Window
{
    public TicTacToeWindow()
    {
        InitializeComponent();
    }

    public void SetViewModel(TicTacToeViewModel viewModel)
    {
        DataContext = viewModel;
        viewModel.RequestClose += (_, _) => Close();
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
}
