using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DrumBuddy.Converters
{
    public class SyncIconVisibleConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 2 &&
                values[0] is bool isSyncEnabled &&
                values[1] is bool isSyncing)
            {
                return isSyncEnabled && !isSyncing;
            }
            return false;
        }

        public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}