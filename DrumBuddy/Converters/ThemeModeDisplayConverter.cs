using System;
using System.Globalization;
using Avalonia.Data.Converters;
using DrumBuddy.Services;

namespace DrumBuddy.Converters;

public class ThemeModeDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is ThemeMode mode
            ? mode switch
            {
                ThemeMode.Light => "Light",
                ThemeMode.Dark => "Dark",
                _ => ""
            }
            : "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}