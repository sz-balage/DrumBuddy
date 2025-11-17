using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using ReactiveUI;

namespace DrumBuddy.Services;

public class NotificationService(Window topLevel) : ReactiveObject
{
    //TODO: make singleton for each window, since with current approach notifications can get behind each other (not using same stack)
    WindowNotificationManager _notificationManager = new(topLevel)
    {
        Position = NotificationPosition.BottomRight
    };

    public void ShowNotification(Notification notification)
    {
        _notificationManager.Show(notification);
    }
}