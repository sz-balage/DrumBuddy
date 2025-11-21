using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DrumBuddy.Converters;

public class IsListeningToBorderBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isListening)
            return isListening
                ? new SolidColorBrush((Color)App.Current?.FindResource("Accent"))
                : App.Current?.FindResource("DarkerGray");
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}