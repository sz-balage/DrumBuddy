using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using DrumBuddy.Client.Services;
using ReactiveUI;

namespace DrumBuddy.Client.Models;

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