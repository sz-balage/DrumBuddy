using System;
using System.Collections.ObjectModel;
using System.Threading;
using Avalonia.Threading;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Client.Models;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.IO.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.Client.ViewModels;

public partial class MainViewModel : ReactiveObject, IScreen
{
    private readonly IMidiService _midiService;
    [Reactive] private bool _connectedSucc;

    [Reactive] private string _errorMessage;

    [Reactive] private bool _isPaneOpen;
    [Reactive] private bool _noConnection;

    [Reactive] private NavigationMenuItemTemplate _selectedPaneItem;

    public MainViewModel()
    {
        _midiService = Locator.Current.GetService<MidiService>();
        _midiService!.InputDeviceDisconnected
            .Subscribe(connected => { NoConnection = true; });
        TryConnect();
        this.WhenAnyValue(vm => vm.SelectedPaneItem)
            .Subscribe(OnSelectedPaneItemChanged);
        ErrorMessage = string.Empty;
    }

    public ObservableCollection<NavigationMenuItemTemplate> PaneItems { get; } = new()
    {
        new NavigationMenuItemTemplate(typeof(HomeViewModel), "HomeIcon"),
        new NavigationMenuItemTemplate(typeof(RecordingViewModel), "RecordIcon"),
        new NavigationMenuItemTemplate(typeof(LibraryViewModel), "LibraryIcon")
    };

    public RoutingState Router { get; } = new();

    private void SuccessfulConnection()
    {
        NoConnection = false;
        ConnectedSucc = true;
        var timer = new Timer(_ => { Dispatcher.UIThread.Invoke(() => ConnectedSucc = false); }, null,
            TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);
    }

    private void ConnectionError(string message)
    {
        NoConnection = true;
        ErrorMessage = message;
        var timer = new Timer(_ => { Dispatcher.UIThread.Invoke(() => ErrorMessage = string.Empty); }, null,
            TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
    }

    private void OnSelectedPaneItemChanged(NavigationMenuItemTemplate? value)
    {
        if (value is null)
            return;
        if (Router.GetCurrentViewModel()?.GetType() == value.ModelType)
            return;
        IsPaneOpen = false;
        Router.NavigateAndReset.Execute(Locator.Current.GetRequiredService(value.ModelType) as IRoutableViewModel);
    }

    [ReactiveCommand]
    private void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    [ReactiveCommand]
    private void TryConnect()
    {
        var connectionResult = _midiService.TryConnect();
        switch (connectionResult.IsSuccess)
        {
            case true:
                SuccessfulConnection();
                break;
            case false:
                ConnectionError(connectionResult.Message!);
                break;
        }
    }
}