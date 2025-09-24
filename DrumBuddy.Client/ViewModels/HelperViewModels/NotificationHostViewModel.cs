using System.Collections.ObjectModel;
using System.Reactive;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Client.Models;
using DrumBuddy.Client.Services;
using ReactiveUI;
using Splat;

namespace DrumBuddy.Client.ViewModels.HelperViewModels;

public partial class NotificationHostViewModel : ReactiveObject
{
    private readonly NotificationService _notificationService;

    public NotificationHostViewModel()
    {
        _notificationService = Locator.Current.GetRequiredService<NotificationService>();
    }
    public ReadOnlyObservableCollection<ToastNotification> ActiveNotifications => _notificationService.ActiveNotifications;

    public ReactiveCommand<ToastNotification, Unit> DismissCommand =>
        ReactiveCommand.Create<ToastNotification>(_notificationService.Dismiss);
}
