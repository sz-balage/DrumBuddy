using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels.HelperViewModels;

namespace DrumBuddy.Views.HelperViews;

public partial class NotificationHost : ReactiveUserControl<NotificationHostViewModel>
{
    public NotificationHost()
    {
        InitializeComponent();
    }
}