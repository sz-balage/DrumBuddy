using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Avalonia.Threading;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Client.Models;
using DrumBuddy.IO.Abstractions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.Client.ViewModels;

public partial class MainViewModel : ReactiveObject, IScreen
{
    private readonly IMidiService _midiService;
    private IDisposable? _successNotificationSub;
    private IDisposable? _successfulConnectionSub;
    private IDisposable? _connectionErrorSub;
    [Reactive] private bool _connectedSucc;

    [Reactive] private string _successMessage;
    [Reactive] private string _errorMessage;

    [Reactive] private bool _isPaneOpen;
    [Reactive] private bool _noConnection;

    [Reactive] private NavigationMenuItemTemplate _selectedPaneItem;

    public MainViewModel(IMidiService midiService)
    {
        _midiService = midiService;
        _midiService!.InputDeviceDisconnected
            .Subscribe(connected => { NoConnection = true; });
        TryConnect();
        this.WhenAnyValue(vm => vm.SelectedPaneItem)
            .Subscribe(OnSelectedPaneItemChanged);
        ErrorMessage = string.Empty;
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

    public void ShowSuccessToastNotification(string message)
    {
        _successNotificationSub?.Dispose();

        SuccessMessage = message;
        _successNotificationSub = DelayedActionSubscription(() => SuccessMessage = string.Empty);
    }
    private void SuccessfulConnection()
    {
        _successfulConnectionSub?.Dispose();
        NoConnection = false;
        ConnectedSucc = true;
        _successfulConnectionSub = DelayedActionSubscription(() => ConnectedSucc = false);
    }

    private void ConnectionError(string message)
    {
        NoConnection = true;
        ErrorMessage = message;
        _connectionErrorSub = DelayedActionSubscription(() => ErrorMessage = string.Empty);
    }

    private IDisposable DelayedActionSubscription(Action action) =>
        Observable.Return(Unit.Default)
            .Delay(DateTimeOffset.Now.AddSeconds(5))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => action.Invoke());

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
                SuccessfulConnection();
                break;
            case false:
                ConnectionError(connectionResult.Message!);
                break;
        }
    }
}