using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using DrumBuddy.Extensions;
using DrumBuddy.IO.Services;
using DrumBuddy.Models;
using DrumBuddy.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels;

public partial class MainViewModel : ReactiveObject, IScreen
{
    private readonly MidiService _midiService;
    [Reactive] private bool _canRetry;
    private IDisposable? _connectionErrorSub;
    [Reactive] private bool _isKeyboardInput;

    [Reactive] private bool _isPaneOpen;
    [Reactive] private bool _noConnection;
    private NotificationService _notificationService;

    [Reactive] private NavigationMenuItemTemplate _selectedPaneItem;
    private IDisposable? _successfulConnectionSub;

    [Reactive] private string _successMessage;
    private IDisposable? _successNotificationSub;

    public MainViewModel(MidiService midiService)
    {
        _midiService = midiService;
        _midiService!.InputDeviceDisconnected
            .Subscribe(connected => { NoConnection = true; });
        this.WhenAnyValue(vm => vm.SelectedPaneItem)
            .Subscribe(OnSelectedPaneItemChanged);
        CanRetry = true;
    }

    public Interaction<MidiDeviceShortInfo[], MidiDeviceShortInfo?> ChooseMidiDevice { get; } = new();
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

    public void SetTopLevelWindow(Window window)
    {
        _notificationService = new NotificationService(window);
    }

    public void NavigateFromCode(IRoutableViewModel viewModel)
    {
        var navigateTo = PaneItems.Single(item => item.ModelType == viewModel.GetType());
        SelectedPaneItem = navigateTo;
    }

    private void SuccessfulConnection(string message)
    {
        NoConnection = false;
        _notificationService.ShowNotification(new Notification("Successful connection.", message,
            NotificationType.Success));
        WindowNotificationManager asd = new();
    }

    private void ConnectionError(string message)
    {
        CanRetry = false;
        NoConnection = true;
        _notificationService.ShowNotification(new Notification("Connection error.", message,
            NotificationType.Error,
            onClose: () => CanRetry = true));
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
        var currentVm = Router.GetCurrentViewModel();
        if (currentVm is RecordingViewModel rvm)
            rvm.Dispose();
        if (currentVm is ConfigurationViewModel cvm)
            cvm.CancelMapping();
        Router.NavigateAndReset.Execute(navigateTo);
    }

    [ReactiveCommand]
    private void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    [ReactiveCommand]
    private async Task TryConnect()
    {
        var connectionResult = _midiService.TryConnect();
        switch (connectionResult.DevicesConnected.Length)
        {
            case 0:
                ConnectionError("No MIDI input devices found. Please connect a device and try again.");
                return;
            case > 1:
                await HandleMultipleMidiDevices(connectionResult.DevicesConnected);
                return;
            default:
                SuccessfulConnection("Connected to " + connectionResult.DevicesConnected[0].Name);
                return;
        }
        // switch (connectionResult.IsSuccess)
        // {
        //     case true:
        //         SuccessfulConnection(connectionResult.Message);
        //         break;
        //     case false:
        //         ConnectionError(connectionResult.Message!);
        //         break;
        // }
    }

    private async Task HandleMultipleMidiDevices(MidiDeviceShortInfo[] deviceInfos)
    {
        var chosenDevice = await ChooseMidiDevice.Handle(deviceInfos);
        if (chosenDevice is null)
        {
            ConnectionError("No device chosen. Please try again in top bar or configuration menu.");
        }
        else
        {
            //TODO: also save to config to persist midi device preference
            _midiService.SetUserChosenDeviceAsInput(chosenDevice);
            SuccessfulConnection("Connected to " + chosenDevice?.Name);
        }
    }
}