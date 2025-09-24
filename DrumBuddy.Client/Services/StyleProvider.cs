using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using DrumBuddy.Client.Models;

namespace DrumBuddy.Client.Services;

public static class StyleProvider
{
    private static readonly StreamGeometry? ErrorIcon = GetStreamGeometryForKey("ErrorIcon");
    private static readonly StreamGeometry? WarningIcon = GetStreamGeometryForKey("WarningIcon");
    private static readonly StreamGeometry? InfoIcon = GetStreamGeometryForKey("InfoIcon");
    private static readonly StreamGeometry? SuccessIcon = GetStreamGeometryForKey("Checkmark");
    
    private static readonly SolidColorBrush? ErrorForeground = GetBrushForKey("Error");
    private static readonly SolidColorBrush? WarningForeground = GetBrushForKey("Warning");
    private static readonly SolidColorBrush? InfoForeground = GetBrushForKey("Accent");
    private static readonly SolidColorBrush? SuccessForeground = GetBrushForKey("AppGreen");
    
    public static SolidColorBrush? GetBrushForKey(string key)
    {
        Application.Current.TryGetResource(key, null, out var obj);
        return new SolidColorBrush((Color)obj);
    }
    public static SolidColorBrush GetForegroundForNotificationType(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.Success:
                return SuccessForeground;
            case NotificationType.Error:
                return ErrorForeground;
            case NotificationType.Warning:
                return WarningForeground;
            default:
                return InfoForeground;
        }
    }
    public static StreamGeometry GetStreamGeometryForNotificationType(NotificationType type)
    { 
        switch (type)
        {
            case NotificationType.Success:
                return SuccessIcon;
            case NotificationType.Error:
                return ErrorIcon;
            case NotificationType.Warning:
                return WarningIcon;
            default:
                return InfoIcon;
        }
    }
    private static StreamGeometry? GetStreamGeometryForKey(string key)
    {
        var styles = Application.Current.Styles.FirstOrDefault(x => x.HasResources && x is not FluentTheme);
        var style = styles.Children.First();
        _ = style.TryGetResource(key, null, out var obj);
        return obj as StreamGeometry;
    }
   
}