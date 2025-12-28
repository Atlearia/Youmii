namespace Youmii.Core.Models;

/// <summary>
/// Represents an item in the radial menu wheel.
/// </summary>
public class RadialMenuItem
{
    /// <summary>
    /// Gets or sets the unique identifier for this menu item.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the icon text (emoji or symbol) for the menu item.
    /// </summary>
    public required string Icon { get; init; }

    /// <summary>
    /// Gets or sets the tooltip/label for the menu item.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Gets or sets the angle position in degrees (0-360) on the wheel.
    /// </summary>
    public double Angle { get; set; }

    /// <summary>
    /// Gets or sets whether this item is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
