using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Youmii.ViewModels;

namespace Youmii
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Position window at top-center of screen
            Loaded += (_, _) =>
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                Left = (screenWidth - Width) / 2;
                Top = 50;

                // Focus input if visible
                if (DataContext is MainViewModel vm && vm.IsInputVisible)
                {
                    InputTextBox.Focus();
                }
            };
        }

        private void Character_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ToggleInputCommand.Execute(null);
                
                if (vm.IsInputVisible)
                {
                    InputTextBox.Focus();
                }
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
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.Dispose();
            }
            base.OnClosed(e);
        }
    }
}