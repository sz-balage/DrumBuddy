using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using DrumBuddy.Api;
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
    private const string LastDeviceKey = "LastUsedMidiDevice";
    private readonly MidiService _midiService;
    private readonly UserService _userService;

    [Reactive] private bool _canRetry;
    private IDisposable? _connectionErrorSub;
    [Reactive] private bool _isAuthenticated; // Add this
    [Reactive] private bool _isKeyboardInput;

    [Reactive] private bool _isPaneOpen;
    [Reactive] private bool _noConnection;
    private NotificationService _notificationService;
    private ConfigurationService _configurationService;

    [Reactive] private NavigationMenuItemTemplate _selectedPaneItem;
    private IDisposable? _successfulConnectionSub;

    [Reactive] private string _successMessage;
    [Reactive] private string _userName;
    private IDisposable? _successNotificationSub;

    public MainViewModel(MidiService midiService, ConfigurationService configurationService)
    {
       
        _configurationService = configurationService;
        _userService = Locator.Current.GetRequiredService<UserService>();
        this.WhenAnyValue(vm => vm.IsAuthenticated)
            .Subscribe(isAuth =>
            {
                UserName = _userService.IsOnline ? _userService.UserName : "Guest";
            });
        IsAuthenticated = _userService.IsOnline;
        _midiService = midiService;
        _midiService!.InputDeviceDisconnected
            .Subscribe(connected => { NoConnection = true; });
        this.WhenAnyValue(vm => vm.SelectedPaneItem)
            .Subscribe(OnSelectedPaneItemChanged); 
        this.WhenAnyValue(vm => vm.IsKeyboardInput)
            .Subscribe(async void (_) =>
            {
                try
                {
                    await TryConnect();
                }
                catch (Exception e)
                {
                    // ignored
                }
            });
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
    [ReactiveCommand]
    private void SignOut()
    {
        _userService.ClearToken();
        _userService.ClearRememberedCredentials();
        IsAuthenticated = false;
    }
    public void SetAuthenticated()
    {
        IsAuthenticated = true;
        var homeItem = PaneItems.First();
        SelectedPaneItem = homeItem;
    }

    public void SetTopLevelWindow(Window window)
    {
        _notificationService = Locator.Current.GetRequiredService<NotificationService>("MainWindowNotificationService");
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
        if (currentVm is ManualViewModel mvm)
            mvm.Reset();
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
        if (IsKeyboardInput)
            return;
        var desiredName = _configurationService.Get<string>(LastDeviceKey) ?? string.Empty;
        var connectionResult = _midiService.TryConnect(desiredName);
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
    }

    public async Task ForceRecheckMidiDevices()
    {
        var desiredName = _configurationService.Get<string>(LastDeviceKey) ?? string.Empty;
        var connectionResult = _midiService.TryConnect(desiredName,true);
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
            _midiService.SetUserChosenDeviceAsInput(chosenDevice);
            await _configurationService.SetAsync(LastDeviceKey, chosenDevice.Name);
            SuccessfulConnection("Connected to " + chosenDevice?.Name);
        }
    }
}