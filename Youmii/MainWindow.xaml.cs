using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Youmii.Behaviors;
using Youmii.Features.ScreenBounds.Views;
using Youmii.ViewModels;

namespace Youmii
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Storyboard? _characterDimIn;
        private Storyboard? _characterDimOut;
        private bool _isRadialMenuHeld;
        private WindowInertiaBehavior? _inertiaBehavior;
        private bool _isDragging;

        public MainWindow()
        {
            InitializeComponent();
            
            // Position window at top-center of screen
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            Left = (screenWidth - Width) / 2;
            Top = 50;

            // Get storyboards
            _characterDimIn = (Storyboard)FindResource("CharacterDimIn");
            _characterDimOut = (Storyboard)FindResource("CharacterDimOut");

            // Initialize inertia behavior
            _inertiaBehavior = new WindowInertiaBehavior(this);
            _inertiaBehavior.BoundaryCollision += OnBoundaryCollision;

            // Focus input if visible
            if (DataContext is MainViewModel vm)
            {
                // Load saved screen bounds
                ApplyScreenBoundsFromSettings(vm);
                
                if (vm.IsInputVisible)
                {
                    InputTextBox.Focus();
                }

                // Subscribe to character dimming changes
                vm.PropertyChanged += OnViewModelPropertyChanged;

                // Subscribe to radial menu events
                vm.RadialMenu.ItemSelected += OnRadialMenuItemSelected;
                vm.RadialMenu.MenuClosed += OnRadialMenuClosed;
                
                // Subscribe to set bounds request
                vm.SetBoundsRequested += OnSetBoundsRequested;
            }
        }

        private void ApplyScreenBoundsFromSettings(MainViewModel vm)
        {
            var (enabled, left, top, right, bottom) = vm.GetScreenBoundsSettings();
            _inertiaBehavior?.SetCustomBounds(enabled, left, top, right, bottom);
        }

        private void OnSetBoundsRequested(object? sender, EventArgs e)
        {
            // Hide main window temporarily
            var wasVisible = Visibility;
            Visibility = Visibility.Hidden;

            // Show the bounds selection overlay
            var overlay = new ScreenBoundsOverlay();
            var result = overlay.ShowDialog();

            // Restore main window
            Visibility = wasVisible;

            if (result == true && overlay.IsConfirmed && DataContext is MainViewModel vm)
            {
                var bounds = overlay.SelectedBounds;
                
                // The overlay returns screen coordinates from PointToScreen
                // These should match Window.Left/Top coordinate system
                var left = bounds.Left;
                var top = bounds.Top;
                var right = bounds.Left + bounds.Width;
                var bottom = bounds.Top + bounds.Height;
                
                // Debug: Show what bounds were captured
                System.Diagnostics.Debug.WriteLine($"Captured bounds: L={left}, T={top}, R={right}, B={bottom}");
                
                // Save bounds to settings
                _ = vm.SaveScreenBoundsAsync(left, top, right, bottom);
                
                // Apply to inertia behavior immediately
                _inertiaBehavior?.SetCustomBounds(true, left, top, right, bottom);
                
                // Show confirmation with the actual values
                if (DataContext is MainViewModel vm2)
                {
                    // Show a bubble with the bounds for debugging
                }
            }
            // Don't show any message if cancelled - just resume normally
        }

        private void OnBoundaryCollision(object? sender, EventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ShowOuchMessage();
            }
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsCharacterDimmed) && sender is MainViewModel vm)
            {
                AnimateCharacterDim(vm.IsCharacterDimmed);
            }
        }

        private void OnRadialMenuItemSelected(object? sender, Core.Models.RadialMenuItem item)
        {
            // Unfade character when item is selected
            if (DataContext is MainViewModel vm)
            {
                vm.IsCharacterDimmed = false;
                vm.HandleRadialMenuItemSelected(item);
            }
            _isRadialMenuHeld = false;
            RadialMenuCanvas.IsHitTestVisible = false;
        }

        private void OnRadialMenuClosed(object? sender, EventArgs e)
        {
            // Unfade character when menu is closed (center clicked or released)
            if (DataContext is MainViewModel vm)
            {
                vm.IsCharacterDimmed = false;
            }
            _isRadialMenuHeld = false;
            RadialMenuCanvas.IsHitTestVisible = false;
        }

        private void AnimateCharacterDim(bool isDimmed)
        {
            if (DataContext is not MainViewModel vm)
                return;
                
            if (isDimmed)
            {
                _characterDimIn?.Begin(CharacterImage);
            }
            else
            {
                // Get the target opacity from settings
                var targetOpacity = vm.CharacterOpacity;
                
                // Configure the animation to animate back to the settings opacity
                if (_characterDimOut != null)
                {
                    var animation = _characterDimOut.Children[0] as DoubleAnimation;
                    if (animation != null)
                    {
                        animation.To = targetOpacity;
                    }
                    _characterDimOut.Begin(CharacterImage);
                }
            }
        }

        #region Left-Click Drag

        private double _dragDpiScale = 1.0;

        private void Character_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ResetIdleTimer();
            }

            // Calculate DPI scale
            int screenPixels = GetSystemMetrics(0); // SM_CXSCREEN
            _dragDpiScale = screenPixels / SystemParameters.PrimaryScreenWidth;

            // Start manual drag with inertia tracking
            _isDragging = true;
            CharacterBorder.CaptureMouse();
            
            // Use window position + mouse position, scaled to pixels
            var mouseInWindow = e.GetPosition(this);
            var screenPos = new Point(
                (Left + mouseInWindow.X) * _dragDpiScale, 
                (Top + mouseInWindow.Y) * _dragDpiScale);
            _inertiaBehavior?.StartDrag(screenPos);
            
            e.Handled = true;
        }

        private void Character_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _inertiaBehavior == null) return;

            // Use window position + mouse position, scaled to pixels
            var mouseInWindow = e.GetPosition(this);
            var screenPos = new Point(
                (Left + mouseInWindow.X) * _dragDpiScale, 
                (Top + mouseInWindow.Y) * _dragDpiScale);
            _inertiaBehavior.UpdateDrag(screenPos);
        }

        private void Character_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging) return;
            
            _isDragging = false;
            CharacterBorder.ReleaseMouseCapture();
            
            // End drag and start inertia animation
            _inertiaBehavior?.EndDrag();
            
            e.Handled = true;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        #endregion

        #region Right-Click Radial Menu

        private void Character_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                // Reset idle timer on character interaction
                vm.ResetIdleTimer();
                
                // Get cursor position relative to the window
                var cursorPosition = e.GetPosition(this);

                // Position the radial menu centered on cursor
                Canvas.SetLeft(RadialMenu, cursorPosition.X);
                Canvas.SetTop(RadialMenu, cursorPosition.Y);

                // Show radial menu
                vm.RadialMenu.ShowAt(cursorPosition.X, cursorPosition.Y);
                vm.IsCharacterDimmed = true;
                _isRadialMenuHeld = true;
                
                // Enable hit testing on the canvas so menu items can be clicked
                RadialMenuCanvas.IsHitTestVisible = true;

                // Capture mouse to detect release anywhere
                CharacterBorder.CaptureMouse();
                
                e.Handled = true;
            }
        }

        private void Character_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm && _isRadialMenuHeld)
            {
                // Release mouse capture
                CharacterBorder.ReleaseMouseCapture();

                // Check if mouse is over a menu item
                var cursorPosition = e.GetPosition(RadialMenu);
                var hitElement = RadialMenu.InputHitTest(cursorPosition) as DependencyObject;

                // Find if we're over a button (menu item)
                Button? menuButton = null;
                while (hitElement != null)
                {
                    if (hitElement is Button btn)
                    {
                        menuButton = btn;
                        break;
                    }
                    hitElement = System.Windows.Media.VisualTreeHelper.GetParent(hitElement);
                }

                if (menuButton != null && menuButton.Command != null && menuButton.Command.CanExecute(menuButton.CommandParameter))
                {
                    // Execute the menu item command
                    menuButton.Command.Execute(menuButton.CommandParameter);
                }
                else
                {
                    // Released without selecting - close menu and unfade
                    vm.RadialMenu.Hide();
                }

                e.Handled = true;
            }
        }

        #endregion

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is MainViewModel vm)
            {
                if (vm.SendCommand.CanExecute(null))
                {
                    vm.SendCommand.Execute(null);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                if (DataContext is MainViewModel vm2)
                {
                    vm2.IsInputVisible = false;
                }
                e.Handled = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+Shift+Y toggles overlay visibility
            if (e.Key == Key.Y && 
                Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.ToggleOverlayCommand.Execute(null);
                }
                e.Handled = true;
            }
            // Escape closes radial menu
            else if (e.Key == Key.Escape && DataContext is MainViewModel vm2 && vm2.RadialMenu.IsVisible)
            {
                vm2.RadialMenu.Hide();
                e.Handled = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.PropertyChanged -= OnViewModelPropertyChanged;
                vm.RadialMenu.ItemSelected -= OnRadialMenuItemSelected;
                vm.RadialMenu.MenuClosed -= OnRadialMenuClosed;
                vm.SetBoundsRequested -= OnSetBoundsRequested;
                vm.Dispose();
            }
            
            // Dispose inertia behavior
            if (_inertiaBehavior != null)
            {
                _inertiaBehavior.BoundaryCollision -= OnBoundaryCollision;
                _inertiaBehavior.Dispose();
                _inertiaBehavior = null;
            }
            
            base.OnClosed(e);
        }
    }
}