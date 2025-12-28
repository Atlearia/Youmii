using Youmii.Core.Models;

namespace Youmii.Core.Interfaces;

/// <summary>
/// Service for managing radial menu items and actions.
/// </summary>
public interface IRadialMenuService
{
    /// <summary>
    /// Gets all available menu items.
    /// </summary>
    IReadOnlyList<RadialMenuItem> GetMenuItems();

    /// <summary>
    /// Executes the action associated with a menu item.
    /// </summary>
    /// <param name="itemId">The menu item identifier.</param>
    /// <returns>True if the action was executed successfully.</returns>
    Task<bool> ExecuteMenuItemAsync(string itemId);
}
