using System;
using Avalonia.Media;
using DrumBuddy.Services;
using ReactiveUI;

namespace DrumBuddy.Models;

public class ToastNotification : ReactiveObject
{
    public ToastNotification(string message, NotificationType type, TimeSpan? duration = null)
    {
        Message = message;
        Type = type;
        Duration = duration ?? TimeSpan.FromSeconds(5);
        Icon = StyleProvider.GetStreamGeometryForNotificationType(type);
        Foreground = StyleProvider.GetForegroundForNotificationType(type);
    }

    public StreamGeometry Icon { get; }
    public SolidColorBrush Foreground { get; }
    public string Message { get; }
    public NotificationType Type { get; }
    public TimeSpan Duration { get; }
}