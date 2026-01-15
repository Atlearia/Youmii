using Youmii.Core.Interfaces;
using Youmii.Core.Models;

namespace Youmii.Core.Services;

/// <summary>
/// Default implementation of the radial menu service with placeholder items.
/// </summary>
public sealed class RadialMenuService : IRadialMenuService
{
    private readonly List<RadialMenuItem> _menuItems;
    private readonly Dictionary<string, Func<Task<bool>>> _actions;

    public RadialMenuService()
    {
        _actions = new Dictionary<string, Func<Task<bool>>>();
        _menuItems = CreateDefaultMenuItems();
    }

    /// <inheritdoc />
    public IReadOnlyList<RadialMenuItem> GetMenuItems() => _menuItems;

    /// <inheritdoc />
    public async Task<bool> ExecuteMenuItemAsync(string itemId)
    {
        if (_actions.TryGetValue(itemId, out var action))
        {
            return await action();
        }

        // Default placeholder action
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Registers an action handler for a menu item.
    /// </summary>
    public void RegisterAction(string itemId, Func<Task<bool>> action)
    {
        _actions[itemId] = action;
    }

    private static List<RadialMenuItem> CreateDefaultMenuItems()
    {
        var items = new List<RadialMenuItem>
        {
            new() { Id = "chat", Icon = "??", Label = "Chat" },
            new() { Id = "settings", Icon = "??", Label = "Settings" },
            new() { Id = "bounds", Icon = "??", Label = "Bounds" },
            new() { Id = "music", Icon = "??", Label = "Music" },
            new() { Id = "games", Icon = "??", Label = "Games" },
            new() { Id = "sleep", Icon = "??", Label = "Sleep" },
        };

        // Calculate angles for even distribution
        var angleStep = 360.0 / items.Count;
        for (int i = 0; i < items.Count; i++)
        {
            // Start from top (-90 degrees) and go clockwise
            items[i].Angle = -90 + (i * angleStep);
        }

        return items;
    }
}
