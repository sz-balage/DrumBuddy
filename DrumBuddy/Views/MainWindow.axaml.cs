using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
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
    private bool isClosingConfirmed;

    public MainWindow()
    {
        _midiService = Locator.Current.GetService<MidiService>();
        ViewModel = Locator.Current.GetService<MainViewModel>();
        Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://DrumBuddy/Assets/app.ico")));
        this.WhenActivated(d =>
        {
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
            this.BindCommand(ViewModel, vm => vm.TogglePaneCommand, v => v.TriggerPaneButton)
                .DisposeWith(d);
            this.Bind(ViewModel, vm => vm.Router, v => v.RoutedViewHost.Router)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.CanRetry, v => v.RetryButton.IsEnabled,
                    string.IsNullOrEmpty)
                .DisposeWith(d);
            // this.Bind(ViewModel, vm => vm.SuccessMessage, v => v.SuccessMessage.Text)
            //     .DisposeWith(d);
            // this.OneWayBind(ViewModel, vm => vm.SuccessMessage, v => v.SuccessBorder.IsVisible,
            //         str => !string.IsNullOrEmpty(str))
            //     .DisposeWith(d);
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
            ViewModel.TryConnectCommand.Execute().Subscribe();
        });
        InitializeComponent();
    }

    public IObservable<int>? KeyboardBeats { get; private set; }

    private RoutedViewHost _routedViewHost => this.FindControl<RoutedViewHost>("RoutedViewHost");
    private Button _retryButton => this.FindControl<Button>("RetryButton");

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