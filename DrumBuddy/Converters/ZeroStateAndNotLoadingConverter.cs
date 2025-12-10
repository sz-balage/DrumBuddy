using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DrumBuddy.Converters;

public class ZeroStateAndNotLoadingConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
            return false;

        var canShowEmptyState = values[0] is true;
        var isLoading = values[1] is true;

        return canShowEmptyState && !isLoading;
    }
}