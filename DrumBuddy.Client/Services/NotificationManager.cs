using Avalonia.Threading;
using DrumBuddy.Client.ViewModels;

namespace DrumBuddy.Client.Services;

public class NotificationManager(MainViewModel mainViewModel)
{
    private MainViewModel _mainViewModel = mainViewModel;
    public void ShowSuccessNotification(string message)
    {
        _mainViewModel.ShowSuccessToastNotification(message);
    }
}