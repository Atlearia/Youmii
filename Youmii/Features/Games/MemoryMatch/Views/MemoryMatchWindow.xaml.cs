using System.Windows;
using System.Windows.Input;
using Youmii.Features.Games.MemoryMatch.ViewModels;

namespace Youmii.Features.Games.MemoryMatch.Views;

/// <summary>
/// Code-behind for Memory Match window.
/// </summary>
public partial class MemoryMatchWindow : Window
{
    public MemoryMatchWindow()
    {
        InitializeComponent();
    }

    public void SetViewModel(MemoryMatchViewModel viewModel)
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
