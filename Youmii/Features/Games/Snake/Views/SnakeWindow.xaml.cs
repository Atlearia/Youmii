using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Youmii.Features.Games.Snake.ViewModels;

namespace Youmii.Features.Games.Snake.Views;

/// <summary>
/// Code-behind for Snake game window.
/// </summary>
public partial class SnakeWindow : Window
{
    private SnakeViewModel? _viewModel;
    private const int CellSize = 20;

    public SnakeWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Focus();
    }

    public void SetViewModel(SnakeViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += (_, _) => Close();
        viewModel.BoardChanged += OnBoardChanged;
        
        // Initial draw
        DrawBoard();
    }

    private void OnBoardChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(DrawBoard);
    }

    private void DrawBoard()
    {
        if (_viewModel == null) return;

        GameCanvas.Children.Clear();

        // Draw grid lines (subtle)
        for (int i = 0; i <= 15; i++)
        {
            var vLine = new Line
            {
                X1 = i * CellSize,
                Y1 = 0,
                X2 = i * CellSize,
                Y2 = 300,
                Stroke = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                StrokeThickness = 1
            };
            GameCanvas.Children.Add(vLine);

            var hLine = new Line
            {
                X1 = 0,
                Y1 = i * CellSize,
                X2 = 300,
                Y2 = i * CellSize,
                Stroke = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                StrokeThickness = 1
            };
            GameCanvas.Children.Add(hLine);
        }

        // Draw food
        var food = _viewModel.GetFoodPosition();
        var foodRect = new Ellipse
        {
            Width = CellSize - 4,
            Height = CellSize - 4,
            Fill = new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Red
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Red,
                BlurRadius = 8,
                ShadowDepth = 0,
                Opacity = 0.5
            }
        };
        Canvas.SetLeft(foodRect, food.X * CellSize + 2);
        Canvas.SetTop(foodRect, food.Y * CellSize + 2);
        GameCanvas.Children.Add(foodRect);

        // Draw snake
        var snakePositions = _viewModel.GetSnakePositions().ToList();
        for (int i = 0; i < snakePositions.Count; i++)
        {
            var pos = snakePositions[i];
            var isHead = i == 0;
            
            var segment = new Rectangle
            {
                Width = CellSize - 2,
                Height = CellSize - 2,
                Fill = isHead 
                    ? new SolidColorBrush(Color.FromRgb(129, 199, 132)) // Light green for head
                    : new SolidColorBrush(Color.FromRgb(76, 175, 80)),  // Green for body
                RadiusX = isHead ? 4 : 2,
                RadiusY = isHead ? 4 : 2
            };

            if (isHead)
            {
                segment.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.LightGreen,
                    BlurRadius = 5,
                    ShadowDepth = 0,
                    Opacity = 0.5
                };
            }

            Canvas.SetLeft(segment, pos.X * CellSize + 1);
            Canvas.SetTop(segment, pos.Y * CellSize + 1);
            GameCanvas.Children.Add(segment);
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        _viewModel?.HandleKeyDown(e.Key);
        e.Handled = true;
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
        if (_viewModel != null)
        {
            _viewModel.BoardChanged -= OnBoardChanged;
        }
        base.OnClosed(e);
    }
}
