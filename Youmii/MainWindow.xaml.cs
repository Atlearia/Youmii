using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
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

            // Focus input if visible
            if (DataContext is MainViewModel vm)
            {
                if (vm.IsInputVisible)
                {
                    InputTextBox.Focus();
                }

                // Subscribe to character dimming changes
                vm.PropertyChanged += OnViewModelPropertyChanged;

                // Subscribe to radial menu events
                vm.RadialMenu.ItemSelected += OnRadialMenuItemSelected;
                vm.RadialMenu.MenuClosed += OnRadialMenuClosed;
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
            if (isDimmed)
            {
                _characterDimIn?.Begin(CharacterImage);
            }
            else
            {
                _characterDimOut?.Begin(CharacterImage);
            }
        }

        private void Character_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                // Get cursor position relative to the window
                var cursorPosition = e.GetPosition(this);

                // Show radial menu at cursor position
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

        private void Character_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                vm.Dispose();
            }
            base.OnClosed(e);
        }
    }
}