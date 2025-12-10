using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using DrumBuddy.Api;
using DrumBuddy.IO.Services;
using DrumBuddy.Models;
using DrumBuddy.Services;
using DrumBuddy.ViewModels;
using DrumBuddy.ViewModels.Dialogs;
using DrumBuddy.Views.Dialogs;
using ReactiveUI;
using Splat;

namespace DrumBuddy.Views;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    private MidiService _midiService;
    private UserService _userService;
    private bool isClosingConfirmed;

    public MainWindow()
    {
        _midiService = Locator.Current.GetService<MidiService>();
        _userService = Locator.Current.GetService<UserService>();
        ViewModel = Locator.Current.GetService<MainViewModel>();
        Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://DrumBuddy/Assets/app.ico")));
        this.WhenActivated(d =>
        {
            var userProfileButton = this.FindControl<Button>("UserProfileButton");
            var signOutButton = this.FindControl<Button>("SignOutButton");
            signOutButton.Click += (_, _) =>
            {
                ViewModel!.SignOutCommand.Execute().Subscribe();

                if (userProfileButton.Flyout?.IsOpen == true)
                    userProfileButton.Flyout.Hide();
            };

            this.OneWayBind(ViewModel, vm => vm.IsAuthenticated, v => v.MainContent.IsVisible)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.IsAuthenticated, v => v.AuthContentPlaceholder.IsVisible, b => !b)
                .DisposeWith(d);
            this.WhenAnyValue(v => v.ViewModel!.IsAuthenticated)
                .DistinctUntilChanged()
                .Subscribe(isAuth =>
                {
                    var authContent = this.FindControl<Grid>("AuthContentPlaceholder");
                    if (!isAuth)
                    {
                        if (authContent != null)
                        {
                            authContent.Children.Clear(); // Dispose old instance
                            authContent.Children.Add(new AuthView { ViewModel = new AuthViewModel() });
                        }
                    }
                    else
                    {
                        if (authContent != null)
                            authContent.Children.Clear(); // Dispose old instance
                    }
                })
                .DisposeWith(d);
            
            this.OneWayBind(ViewModel, vm => vm.PaneItems, v => v.PaneListBox.ItemsSource)
                .DisposeWith(d);

            ViewModel.WhenAnyValue(vm => vm.NoConnection, vm => vm.IsKeyboardInput).Subscribe(values =>
            {
                NoConnection.IsVisible = values.Item1 && !values.Item2;
            });
            this.Bind(ViewModel, vm => vm.SelectedPaneItem, v => v.PaneListBox.SelectedItem)
                .DisposeWith(d);
            this.Bind(ViewModel, vm => vm.IsPaneOpen, v => v.SplitView.IsPaneOpen)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.TogglePaneCommand, v => v.OpenPaneButton)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.TogglePaneCommand, v => v.ClosePaneButton)
                .DisposeWith(d);
            this.Bind(ViewModel, vm => vm.Router, v => v.RoutedViewHost.Router)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.CanRetry, v => v.RetryButton.IsEnabled,
                    string.IsNullOrEmpty)
                .DisposeWith(d);
       
            this.BindCommand(ViewModel, vm => vm.TryConnectCommand, v => v._retryButton)
                .DisposeWith(d);
            this.BindInteraction(ViewModel, vm => vm.ChooseMidiDevice, HandleMidiDeviceChoosing).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.IsKeyboardInput, v => v.ModeIndicatorIcon.Data,
                StyleProvider.GetStreamGeometryForInputType);
            this.OneWayBind(ViewModel, vm => vm.IsKeyboardInput, v => v.ModeIndicatorText.Text,
                ki => ki ? "Keyboard" : "MIDI");
            Closing += async (_, e) =>
            {
                if (isClosingConfirmed)
                    return;
                if (ViewModel?.CurrentViewModel is ManualViewModel manualVm)
                {
                    var editorVm = manualVm.Editor;
                    if (!editorVm?.IsSaved ?? false)
                    {
                        e.Cancel = true;

                        var result = await editorVm.ShowConfirmation.Handle(Unit.Default);

                        if (result == Confirmation.Discard)
                        {
                            isClosingConfirmed = true;
                            Close();
                        }
                    }
                }
            };
            KeyboardBeats = Observable.FromEventPattern(this, nameof(KeyDown))
                .Select(ep => ep.EventArgs as KeyEventArgs)
                .Select(e => KeyboardBeatProvider.GetDrumValueForKey(e.Key));
            ViewModel.SetTopLevelWindow(this);
            if (ViewModel.IsAuthenticated) TryConnectToMidi();
        });
        InitializeComponent();
    }

    public IObservable<int>? KeyboardBeats { get; private set; }

    private RoutedViewHost _routedViewHost => this.FindControl<RoutedViewHost>("RoutedViewHost");
    private Button _retryButton => this.FindControl<Button>("RetryButton");

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Root.Opacity = 1;
    }

    private void TryConnectToMidi()
    {
        ViewModel?.TryConnectCommand.Execute().Subscribe();
    }

    private async Task HandleMidiDeviceChoosing(
        IInteractionContext<MidiDeviceShortInfo[], MidiDeviceShortInfo?> context)
    {
        var dialog = new MidiDeviceChooserView
        {
            ViewModel = new MidiDeviceChooserViewModel(context.Input)
        };
        var result = await dialog.ShowDialog<MidiDeviceShortInfo?>(this);
        context.SetOutput(result);
    }
}