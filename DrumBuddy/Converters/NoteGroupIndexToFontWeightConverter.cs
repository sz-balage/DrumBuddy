using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DrumBuddy.Converters;

public class NoteGroupIndexToFontWeightConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not int idx ? null :
            (idx - 1) % 4 == 0 ? FontWeight.Bold : FontWeight.Normal;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}