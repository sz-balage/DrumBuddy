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

        // values[0] is Sheets.Count
        // values[1] is IsLoadingSheets
        
        var sheetsCount = values[0] is int count ? count : 0;
        var isLoading = values[1] is bool loading ? loading : false;

        return sheetsCount == 0 && !isLoading;
    }
}