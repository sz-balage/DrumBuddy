using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using DrumBuddy.Models;

namespace DrumBuddy.Services;

public static class StyleProvider
{

    private static readonly StreamGeometry? MidiIcon = GetStreamGeometryForKey("music");
    private static readonly StreamGeometry? KeyboardIcon = GetStreamGeometryForKey("Keyboard");


    public static SolidColorBrush? GetBrushForKey(string key)
    {
        Application.Current.TryGetResource(key, null, out var obj);
        return new SolidColorBrush((Color)obj);
    }

    public static StreamGeometry GetStreamGeometryForInputType(bool isKeyboardInput) => isKeyboardInput ? KeyboardIcon : MidiIcon;

    private static StreamGeometry? GetStreamGeometryForKey(string key)
    {
        var styles = Application.Current.Styles.FirstOrDefault(x => x.HasResources && x is not FluentTheme);
        var style = styles.Children.First();
        _ = style.TryGetResource(key, null, out var obj);
        return obj as StreamGeometry;
    }
}