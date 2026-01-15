using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Youmii.Features.ScreenBounds.Views;

/// <summary>
/// Full-screen overlay for selecting custom screen bounds.
/// </summary>
public partial class ScreenBoundsOverlay : Window
{
    private Point _startPoint;
    private bool _isSelecting;
    private Rect _selectedBounds;
    private double _dpiScale = 1.0;

    /// <summary>
    /// Gets the selected bounds in device pixels (matching actual screen coordinates).
    /// </summary>
    public Rect SelectedBounds => _selectedBounds;

    /// <summary>
    /// Gets whether the user confirmed a selection.
    /// </summary>
    public bool IsConfirmed { get; private set; }

    public ScreenBoundsOverlay()
    {
        InitializeComponent();
        
        // Get actual screen size in pixels
        int screenWidthPixels = GetSystemMetrics(SM_CXSCREEN);
        int screenHeightPixels = GetSystemMetrics(SM_CYSCREEN);
        
        // Calculate DPI scale by comparing actual pixels to WPF logical units
        double wpfWidth = SystemParameters.PrimaryScreenWidth;
        _dpiScale = screenWidthPixels / wpfWidth;
        
        // Set window to cover entire screen in WPF logical units
        // WPF will scale this to actual pixels
        Width = wpfWidth;
        Height = SystemParameters.PrimaryScreenHeight;
        Left = 0;
        Top = 0;

        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        KeyDown += OnKeyDown;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(this);
        _isSelecting = true;
        
        InstructionsPanel.Visibility = Visibility.Collapsed;
        SelectionBorder.Visibility = Visibility.Visible;
        SizeIndicator.Visibility = Visibility.Visible;
        
        Canvas.SetLeft(SelectionBorder, _startPoint.X);
        Canvas.SetTop(SelectionBorder, _startPoint.Y);
        SelectionBorder.Width = 0;
        SelectionBorder.Height = 0;

        CaptureMouse();
        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isSelecting) return;

        var currentPoint = e.GetPosition(this);

        var left = Math.Min(_startPoint.X, currentPoint.X);
        var top = Math.Min(_startPoint.Y, currentPoint.Y);
        var width = Math.Abs(currentPoint.X - _startPoint.X);
        var height = Math.Abs(currentPoint.Y - _startPoint.Y);

        Canvas.SetLeft(SelectionBorder, left);
        Canvas.SetTop(SelectionBorder, top);
        SelectionBorder.Width = width;
        SelectionBorder.Height = height;

        // Show size in actual pixels (scaled up)
        var pixelWidth = width * _dpiScale;
        var pixelHeight = height * _dpiScale;
        SizeText.Text = $"{pixelWidth:F0} x {pixelHeight:F0}";
        Canvas.SetLeft(SizeIndicator, left);
        Canvas.SetTop(SizeIndicator, Math.Max(0, top - 30));

        UpdateOverlayWithCutout(left, top, width, height);
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelecting) return;

        _isSelecting = false;
        ReleaseMouseCapture();

        var currentPoint = e.GetPosition(this);

        // Get bounds in WPF logical units
        var left = Math.Min(_startPoint.X, currentPoint.X);
        var top = Math.Min(_startPoint.Y, currentPoint.Y);
        var width = Math.Abs(currentPoint.X - _startPoint.X);
        var height = Math.Abs(currentPoint.Y - _startPoint.Y);

        // Convert to actual pixels for storage
        var pixelLeft = left * _dpiScale;
        var pixelTop = top * _dpiScale;
        var pixelWidth = width * _dpiScale;
        var pixelHeight = height * _dpiScale;

        // Require minimum size (200x200 in pixels)
        if (pixelWidth >= 200 && pixelHeight >= 200)
        {
            // Store in PIXEL coordinates
            _selectedBounds = new Rect(pixelLeft, pixelTop, pixelWidth, pixelHeight);
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }
        else
        {
            SelectionBorder.Visibility = Visibility.Collapsed;
            SizeIndicator.Visibility = Visibility.Collapsed;
            InstructionsPanel.Visibility = Visibility.Visible;
            ResetOverlay();
            
            MessageBox.Show("Selection too small. Please select an area at least 200x200 pixels.",
                          "Selection Too Small", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }
    }

    private void UpdateOverlayWithCutout(double left, double top, double width, double height)
    {
        var screenRect = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight));
        var selectionRect = new RectangleGeometry(new Rect(left, top, width, height));

        var combinedGeometry = new CombinedGeometry(
            GeometryCombineMode.Exclude,
            screenRect,
            selectionRect);

        DarkOverlay.Clip = null;
        DarkOverlay.Fill = new SolidColorBrush(Color.FromArgb(136, 0, 0, 0));
        
        var path = new Path
        {
            Fill = new SolidColorBrush(Color.FromArgb(136, 0, 0, 0)),
            Data = combinedGeometry
        };

        var grid = (Grid)Content;
        grid.Children.Remove(DarkOverlay);
        
        var existingPath = grid.Children.OfType<Path>().FirstOrDefault();
        if (existingPath != null)
            grid.Children.Remove(existingPath);
        
        grid.Children.Insert(0, path);
    }

    private void ResetOverlay()
    {
        var grid = (Grid)Content;
        
        var existingPath = grid.Children.OfType<Path>().FirstOrDefault();
        if (existingPath != null)
            grid.Children.Remove(existingPath);
        
        if (!grid.Children.Contains(DarkOverlay))
            grid.Children.Insert(0, DarkOverlay);
        
        DarkOverlay.Fill = new SolidColorBrush(Color.FromArgb(136, 0, 0, 0));
    }

    #region Win32 Interop

    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    #endregion
}
