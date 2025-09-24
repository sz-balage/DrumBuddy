using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Avalonia.Threading;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Client.Models;
using DrumBuddy.Client.Services;
using DrumBuddy.Client.ViewModels.HelperViewModels;
using DrumBuddy.IO.Abstractions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.Client.ViewModels;

public partial class MainViewModel : ReactiveObject, IScreen
{
    private readonly IMidiService _midiService;
    private readonly NotificationService _notificationService;
    private IDisposable? _successNotificationSub;
    private IDisposable? _successfulConnectionSub;
    private IDisposable? _connectionErrorSub;

    [Reactive] private string _successMessage;

    [Reactive] private bool _isPaneOpen;
    [Reactive] private bool _canRetry;
    [Reactive] private bool _noConnection;

    [Reactive] private NavigationMenuItemTemplate _selectedPaneItem;
    public MainViewModel(IMidiService midiService, NotificationService notificationService)
    {
        _midiService = midiService;
        _notificationService = notificationService;
        _midiService!.InputDeviceDisconnected
            .Subscribe(connected => { NoConnection = true; });
        TryConnect();
        this.WhenAnyValue(vm => vm.SelectedPaneItem)
            .Subscribe(OnSelectedPaneItemChanged);
        CanRetry = true;
    }
    public IRoutableViewModel CurrentViewModel { get; private set; }

    public ObservableCollection<NavigationMenuItemTemplate> PaneItems { get; } = new()
    {
        new NavigationMenuItemTemplate(typeof(HomeViewModel), "HomeIcon", ""),
        new NavigationMenuItemTemplate(typeof(RecordingViewModel), "RecordIcon", "Record your beats in a new sheet."),
        new NavigationMenuItemTemplate(typeof(LibraryViewModel), "LibraryIcon", "Review and manage your saved sheets."),
        new NavigationMenuItemTemplate(typeof(ManualViewModel), "EditorIcon",
            "Manually create a new sheet, or edit an existing one for a more precise structure."),
        new NavigationMenuItemTemplate(typeof(ConfigurationViewModel), "SettingsIcon",
            "Add, or change your drum mapping configuration.")
    };

    public RoutingState Router { get; } = new();

    public void NavigateFromCode(IRoutableViewModel viewModel)
    {
        var navigateTo = PaneItems.Single(item => item.ModelType == viewModel.GetType());
        SelectedPaneItem = navigateTo;
    }
    private void SuccessfulConnection(string message)
    {
        NoConnection = false;
        _notificationService.ShowNotification(message, NotificationType.Success);
    }

    private void ConnectionError(string message)
    {
        CanRetry = false;
        NoConnection = true;
        _notificationService.ShowNotification(message, 
            NotificationType.Error,
            onNotificationDismissed: () => CanRetry = true);
    }

    private void OnSelectedPaneItemChanged(NavigationMenuItemTemplate? value)
    {
        if (value is null)
            return;
        if (Router.GetCurrentViewModel()?.GetType() == value.ModelType)
            return;
        IsPaneOpen = false;
        var navigateTo = Locator.Current.GetRequiredService(value.ModelType) as IRoutableViewModel;
        if (navigateTo is null)
            throw new Exception("ViewModel not found.");
        CurrentViewModel = navigateTo;
        if (Router.GetCurrentViewModel() is RecordingViewModel rvm)
            rvm.Dispose();
        Router.NavigateAndReset.Execute(navigateTo);
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
                SuccessfulConnection(connectionResult.Message);
                break;
            case false:
                ConnectionError(connectionResult.Message!);
                break;
        }
    }
}