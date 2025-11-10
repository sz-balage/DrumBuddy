using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DrumBuddy.Services;

namespace DrumBuddy.Converters;

public class ButtonIconEnabledStateForegroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not bool enabled ? null : enabled ? Brushes.Black : Brushes.Gray;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}