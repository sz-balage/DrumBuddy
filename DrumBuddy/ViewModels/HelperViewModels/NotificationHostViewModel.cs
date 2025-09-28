using System.Collections.ObjectModel;
using System.Reactive;
using DrumBuddy.Extensions;
using DrumBuddy.Models;
using DrumBuddy.Services;
using ReactiveUI;
using Splat;

namespace DrumBuddy.ViewModels.HelperViewModels;

public class NotificationHostViewModel : ReactiveObject
{
    private readonly NotificationService _notificationService;

    public NotificationHostViewModel()
    {
        _notificationService = Locator.Current.GetRequiredService<NotificationService>();
    }

    public ReadOnlyObservableCollection<ToastNotification> ActiveNotifications =>
        _notificationService.ActiveNotifications;

    public ReactiveCommand<ToastNotification, Unit> DismissCommand =>
        ReactiveCommand.Create<ToastNotification>(_notificationService.Dismiss);
}