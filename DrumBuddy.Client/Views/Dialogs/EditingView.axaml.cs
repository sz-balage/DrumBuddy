using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.DesignHelpers;
using DrumBuddy.Client.Models;
using DrumBuddy.Client.ViewModels.Dialogs;
using DrumBuddy.Client.Views.HelperViews;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using ReactiveUI;
using Splat;

namespace DrumBuddy.Client.Views.Dialogs;

public partial class EditingView : ReactiveWindow<EditingViewModel>
{

    public EditingView()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            var sheet = new Sheet(100, ImmutableArray<Measure>.Empty, "", "");
            ViewModel = new EditingViewModel(TestSheetProvider.GetTestSheet(), null, null, true);
        }
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.Measures, v => v.MeasureControl.ItemsSource)
                .DisposeWith(d);
            //editing stuff
            if (!ViewModel.IsViewOnly)
            {
                this.BindCommand(ViewModel, vm => vm.StartRecordingCommand, v => v._startRecordingButton)
                    .DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StopRecordingCommand, v => v._stopRecordingButton)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsRecording, v => v._bpmNumeric.IsEnabled, i => !i)
                    .DisposeWith(d);
                this.OneWayBind(
                    ViewModel,
                    vm => vm.IsRecording,
                    v => v._startRecordingButton.IsVisible,
                    i => { return !i; });
                this.OneWayBind(ViewModel, vm => vm.IsRecording, v => v._stopRecordingButton.IsVisible,
                    i => { return i; });
                this.Bind(ViewModel, vm => vm.BpmDecimal, v => v._bpmNumeric.Value);
                this.Bind(ViewModel, vm => vm.TimeElapsed, v => v._timeElapsedTB.Text);
                this.OneWayBind(ViewModel, vm => vm.CountDown, v => v._countDownTB.Text)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CountDownVisibility, v => v._countDownGrid.IsVisible)
                    .DisposeWith(d);
                ViewModel!.KeyboardBeats = Observable.FromEventPattern(this, nameof(KeyDown))
                    .Select(ep => ep.EventArgs as KeyEventArgs)
                    .Select(e => e.Key switch
                    {
                        Key.A => (int)Drum.HiHat,
                        Key.S => (int)Drum.Snare,
                        Key.D => (int)Drum.Kick,
                        Key.F => (int)Drum.FloorTom,
                        Key.Q => (int)Drum.Crash,
                        Key.W => (int)Drum.Tom1,
                        Key.E => (int)Drum.Tom2,
                        Key.R => (int)Drum.Ride,
                        _ => -2
                    });
                ViewModel!.StopRecordingCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine(ex.Message));
                ViewModel.WhenAnyValue(vm => vm.CurrentMeasure).Subscribe(measure =>
                {
                    // Find the container for the current measure
                    var idx = MeasureControl.Items.IndexOf(measure);
                    var container = MeasureControl.ContainerFromIndex(idx + 3);
                    if (container != null)
                        // Scroll the container into view
                        container.BringIntoView();
                });
            }
        });
    }

    private ItemsControl MeasureControl => this.FindControl<ItemsControl>("MeasuresItemControl")!;
    private Button _startRecordingButton => this.FindControl<Button>("StartRecordingButton")!;
    private Button _stopRecordingButton => this.FindControl<Button>("StopRecordingButton")!;
    private Button _pauseRecordingButton => this.FindControl<Button>("PauseRecordingButton")!;
    private Button _resumeRecordingButton => this.FindControl<Button>("ResumeRecordingButton")!;
    private NumericUpDown _bpmNumeric => this.FindControl<NumericUpDown>("BpmNumeric")!;
    private TextBlock _timeElapsedTB => this.FindControl<TextBlock>("TimeElapsedTextBlock")!;
    private TextBlock _countDownTB => this.FindControl<TextBlock>("CountdownTextBlock")!;
    private Grid _countDownGrid => this.FindControl<Grid>("CountdownGrid")!;
    private CheckBox _keyboardCheckBox => this.FindControl<CheckBox>("KeyboardInputCheckBox")!;

    private async Task SaveHandler(IInteractionContext<SheetCreationData, string?> context)
    {
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var saveView = new SaveSheetView { ViewModel = new SaveSheetViewModel(context.Input) };
        var result = await saveView.ShowDialog<string>(mainWindow);
        context.SetOutput(result);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is MeasureView measureView)
        {
            var index = MeasureControl.ItemContainerGenerator.IndexFromContainer(
                measureView.Parent as Control);

            ViewModel?.HandleMeasureClick(index);
        }
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var result = ViewModel.Save();
        Close(result);
    }
}