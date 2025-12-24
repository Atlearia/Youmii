using System.Windows;
using System.Windows.Input;

namespace Youmii.Behaviors;

/// <summary>
/// Attached behavior that enables dragging the parent window by clicking and dragging an element.
/// </summary>
public static class DragBehavior
{
    public static readonly DependencyProperty EnableDragProperty =
        DependencyProperty.RegisterAttached(
            "EnableDrag",
            typeof(bool),
            typeof(DragBehavior),
            new PropertyMetadata(false, OnEnableDragChanged));

    private static Point _dragStartPoint;
    private static bool _isDragging;

    public static bool GetEnableDrag(DependencyObject obj) => (bool)obj.GetValue(EnableDragProperty);
    public static void SetEnableDrag(DependencyObject obj, bool value) => obj.SetValue(EnableDragProperty, value);

    private static void OnEnableDragChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if ((bool)e.NewValue)
        {
            element.MouseLeftButtonDown += OnMouseLeftButtonDown;
            element.MouseMove += OnMouseMove;
            element.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }
        else
        {
            element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            element.MouseMove -= OnMouseMove;
            element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        }
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement element)
            return;

        var window = Window.GetWindow(element);
        if (window == null)
            return;

        _isDragging = true;
        _dragStartPoint = e.GetPosition(window);
        element.CaptureMouse();
        e.Handled = true;
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || sender is not UIElement element)
            return;

        var window = Window.GetWindow(element);
        if (window == null)
            return;

        var currentPoint = e.GetPosition(window);
        window.Left += currentPoint.X - _dragStartPoint.X;
        window.Top += currentPoint.Y - _dragStartPoint.Y;
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging || sender is not UIElement element)
            return;

        _isDragging = false;
        element.ReleaseMouseCapture();
        e.Handled = true;
    }
}