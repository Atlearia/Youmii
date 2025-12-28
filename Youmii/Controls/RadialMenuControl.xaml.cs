using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Youmii.ViewModels;

namespace Youmii.Controls;

/// <summary>
/// Interaction logic for RadialMenuControl.xaml
/// </summary>
public partial class RadialMenuControl : UserControl
{
    private Storyboard? _fadeInStoryboard;
    private Storyboard? _fadeOutStoryboard;

    public RadialMenuControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _fadeInStoryboard = (Storyboard)FindResource("FadeInStoryboard");
        _fadeOutStoryboard = (Storyboard)FindResource("FadeOutStoryboard");

        // Subscribe to ViewModel changes
        if (DataContext is RadialMenuViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
            UpdateVisibility(vm.IsVisible);
            UpdatePosition(vm);
        }

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is RadialMenuViewModel oldVm)
        {
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (e.NewValue is RadialMenuViewModel newVm)
        {
            newVm.PropertyChanged += OnViewModelPropertyChanged;
            UpdateVisibility(newVm.IsVisible);
            UpdatePosition(newVm);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not RadialMenuViewModel vm) return;

        switch (e.PropertyName)
        {
            case nameof(RadialMenuViewModel.IsVisible):
                UpdateVisibility(vm.IsVisible);
                break;
            case nameof(RadialMenuViewModel.PositionX):
            case nameof(RadialMenuViewModel.PositionY):
                UpdatePosition(vm);
                break;
        }
    }

    private void UpdatePosition(RadialMenuViewModel vm)
    {
        // Position is set via Canvas.Left/Top in the parent, 
        // but we also update render transform for centering
        // The control itself centers at the position
    }

    private void UpdateVisibility(bool isVisible)
    {
        if (isVisible)
        {
            ShowMenu();
        }
        else
        {
            HideMenu();
        }
    }

    private void ShowMenu()
    {
        RootGrid.IsHitTestVisible = true;
        _fadeInStoryboard?.Begin(RootGrid);
    }

    private void HideMenu()
    {
        if (_fadeOutStoryboard != null)
        {
            var storyboard = _fadeOutStoryboard.Clone();
            storyboard.Completed += (_, _) =>
            {
                RootGrid.IsHitTestVisible = false;
            };
            storyboard.Begin(RootGrid);
        }
        else
        {
            RootGrid.IsHitTestVisible = false;
            RootGrid.Opacity = 0;
        }
    }
}
