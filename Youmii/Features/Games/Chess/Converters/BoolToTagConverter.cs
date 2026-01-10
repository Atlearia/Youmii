using System.Globalization;
using System.Windows.Data;

namespace Youmii.Features.Games.Chess.Converters;

/// <summary>
/// Converts a boolean to "Selected" tag for difficulty buttons.
/// </summary>
public sealed class BoolToTagConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? "Selected" : null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() == "Selected";
    }
}
