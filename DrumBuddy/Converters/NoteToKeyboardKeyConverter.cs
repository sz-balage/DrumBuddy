using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DrumBuddy.Services;

namespace DrumBuddy.Converters;

public class NoteToKeyboardKeyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        value is not int note ? null : KeyboardBeatProvider.GetKeyForDrumValue(note);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}