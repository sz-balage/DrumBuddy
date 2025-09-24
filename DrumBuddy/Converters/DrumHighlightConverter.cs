using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DrumBuddy.Services;

namespace DrumBuddy.Converters;

public class DrumHighlightConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object parameter, CultureInfo culture)
    {
        bool isUnmapped = values[0] is bool b1 && b1;
        bool isHighlighted = values[1] is bool b2 && b2;
        var appGreenBrush = StyleProvider.GetBrushForKey("AppGreen");
        var errorBrush = StyleProvider.GetBrushForKey("Error");
        if (isHighlighted)
            return appGreenBrush;
        if (isUnmapped)
            return errorBrush;
        return Brushes.Transparent;
    }
}