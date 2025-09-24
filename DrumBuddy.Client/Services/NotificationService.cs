using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using DrumBuddy.Client.Models;
using DynamicData;
using ReactiveUI;
using NotificationType = DrumBuddy.Client.Models.NotificationType;

namespace DrumBuddy.Client.Services;

public class NotificationService : ReactiveObject
{
    private readonly SourceList<ToastNotification> _notifications = new();
    private readonly ReadOnlyObservableCollection<ToastNotification> _activeNotifications;

    public NotificationService()
    {
        _notifications.Connect()
            .Bind(out _activeNotifications)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe();
    }

    public ReadOnlyObservableCollection<ToastNotification> ActiveNotifications => _activeNotifications;

    public void ShowNotification(string message, NotificationType type,Action onNotificationDismissed = null, TimeSpan? duration = null)
    {
        var notification = new ToastNotification(message, type, duration);
        _notifications.Add(notification);

        Observable.Timer(notification.Duration)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                Dismiss(notification);
                onNotificationDismissed?.Invoke();
            });
    }

    public void Dismiss(ToastNotification notification)
    {
        _notifications.Remove(notification);
    }
}