using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.ViewModels.HelperViewModels;

namespace DrumBuddy.Client.Views.HelperViews;

public partial class NotificationHost : ReactiveUserControl<NotificationHostViewModel>
{
    public NotificationHost()
    {
        InitializeComponent();
    }
}