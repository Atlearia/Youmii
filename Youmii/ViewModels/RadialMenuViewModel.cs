using System.Collections.ObjectModel;
using System.Windows.Input;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;

namespace Youmii.ViewModels;

/// <summary>
/// ViewModel for the radial menu wheel.
/// Handles menu visibility, item selection, and animations.
/// </summary>
public sealed class RadialMenuViewModel : ViewModelBase
{
    private readonly IRadialMenuService _menuService;
    private bool _isVisible;
    private RadialMenuItem? _selectedItem;
    private double _positionX;
    private double _positionY;

    public RadialMenuViewModel(IRadialMenuService menuService)
    {
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        
        MenuItems = new ObservableCollection<RadialMenuItemViewModel>();
        LoadMenuItems();

        SelectItemCommand = new RelayCommand<string>(SelectItem);
        CloseMenuCommand = new RelayCommand(CloseMenu);
    }

    /// <summary>
    /// Gets or sets whether the radial menu is visible.
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    /// <summary>
    /// Gets or sets the X position of the menu center.
    /// </summary>
    public double PositionX
    {
        get => _positionX;
        set => SetProperty(ref _positionX, value);
    }

    /// <summary>
    /// Gets or sets the Y position of the menu center.
    /// </summary>
    public double PositionY
    {
        get => _positionY;
        set => SetProperty(ref _positionY, value);
    }

    /// <summary>
    /// Gets or sets the currently selected menu item.
    /// </summary>
    public RadialMenuItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    /// <summary>
    /// Gets the collection of menu item view models.
    /// </summary>
    public ObservableCollection<RadialMenuItemViewModel> MenuItems { get; }

    /// <summary>
    /// Gets the radius of the radial menu wheel.
    /// </summary>
    public double Radius => 80;

    /// <summary>
    /// Command to select a menu item by ID.
    /// </summary>
    public ICommand SelectItemCommand { get; }

    /// <summary>
    /// Command to close the menu.
    /// </summary>
    public ICommand CloseMenuCommand { get; }

    /// <summary>
    /// Event raised when a menu item is selected.
    /// </summary>
    public event EventHandler<RadialMenuItem>? ItemSelected;

    /// <summary>
    /// Event raised when the menu is closed without selecting an item.
    /// </summary>
    public event EventHandler? MenuClosed;

    /// <summary>
    /// Shows the menu at the specified position.
    /// </summary>
    public void ShowAt(double x, double y)
    {
        PositionX = x;
        PositionY = y;
        IsVisible = true;
    }

    /// <summary>
    /// Hides the menu.
    /// </summary>
    public void Hide()
    {
        if (IsVisible)
        {
            IsVisible = false;
            MenuClosed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Toggles the menu visibility at the specified position.
    /// </summary>
    public void ToggleAt(double x, double y)
    {
        if (IsVisible)
        {
            Hide();
        }
        else
        {
            ShowAt(x, y);
        }
    }

    private void LoadMenuItems()
    {
        MenuItems.Clear();
        var items = _menuService.GetMenuItems();
        
        foreach (var item in items)
        {
            MenuItems.Add(new RadialMenuItemViewModel(item, Radius));
        }
    }

    private void SelectItem(string? itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        var itemVm = MenuItems.FirstOrDefault(m => m.Item.Id == itemId);
        if (itemVm != null)
        {
            SelectedItem = itemVm.Item;
            ItemSelected?.Invoke(this, itemVm.Item);
            
            // Execute the menu action asynchronously
            _ = _menuService.ExecuteMenuItemAsync(itemId);
        }

        // Hide menu after selection
        IsVisible = false;
    }

    private void CloseMenu()
    {
        Hide();
    }
}

/// <summary>
/// ViewModel for individual radial menu items with position calculation.
/// </summary>
public sealed class RadialMenuItemViewModel : ViewModelBase
{
    public RadialMenuItemViewModel(RadialMenuItem item, double radius)
    {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        
        // Calculate X, Y position based on angle and radius
        var angleRad = item.Angle * Math.PI / 180.0;
        X = Math.Cos(angleRad) * radius;
        Y = Math.Sin(angleRad) * radius;
    }

    /// <summary>
    /// Gets the underlying menu item model.
    /// </summary>
    public RadialMenuItem Item { get; }

    /// <summary>
    /// Gets the X position offset from center.
    /// </summary>
    public double X { get; }

    /// <summary>
    /// Gets the Y position offset from center.
    /// </summary>
    public double Y { get; }
}
