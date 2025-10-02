using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using DrumBuddy.Models;
using DrumBuddy.Services;
using DrumBuddy.ViewModels;
using DrumBuddy.ViewModels.Dialogs;
using DrumBuddy.Views.Dialogs;
using DrumBuddy.Views.HelperViews;
using ReactiveUI;
using Splat;

// ReSharper disable InconsistentNaming

namespace DrumBuddy.Views;

public partial class RecordingView : ReactiveUserControl<RecordingViewModel>
{
    public RecordingView()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
            ViewModel = new RecordingViewModel(null, null, null, null, null, null);
        this.WhenActivated(d =>
        {
            if (Design.IsDesignMode)
                return;
            // this.OneWayBind(ViewModel, vm => vm.Measures, v => v.MeasureControl.ItemsSource)
            //     .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.StartRecordingCommand, v => v._startRecordingButton)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.StopRecordingCommand, v => v._stopRecordingButton)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.PauseRecordingCommand, v => v._pauseRecordingButton)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.ResumeRecordingCommand, v => v._resumeRecordingButton)
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
            ViewModel.WhenAnyValue(vm => vm.IsRecording, vm => vm.IsPaused)
                .Subscribe(rp =>
                {
                    if (rp.Item1 && !rp.Item2)
                        _pauseRecordingButton.IsVisible = true;
                    else
                        _pauseRecordingButton.IsVisible = false;
                })
                .DisposeWith(d);
            ViewModel.WhenAnyValue(vm => vm.IsRecording, vm => vm.IsPaused)
                .Subscribe(rp =>
                {
                    if (rp.Item1 && rp.Item2)
                        _resumeRecordingButton.IsVisible = true;
                    else
                        _resumeRecordingButton.IsVisible = false;
                })
                .DisposeWith(d);
            ViewModel.WhenAnyValue(vm => vm.IsRecording, vm => vm.IsLoadingSheets)
                .Subscribe(vals =>
                {
                    if (!vals.Item1 && !vals.Item2)
                        OverlaySheetComboBox.IsEnabled = true;
                    else
                        OverlaySheetComboBox.IsEnabled = false;
                });
            this.Bind(ViewModel, vm => vm.BpmDecimal, v => v._bpmNumeric.Value);
            this.Bind(ViewModel, vm => vm.TimeElapsed, v => v._timeElapsedTB.Text);
            this.OneWayBind(ViewModel, vm => vm.CountDown, v => v._countDownTB.Text)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.CountDownVisibility, v => v._countDownGrid.IsVisible)
                .DisposeWith(d);
            this.BindInteraction(ViewModel, vm => vm.ShowSaveDialog, SaveHandler);
            ViewModel!.KeyboardBeats = Observable.FromEventPattern(this, nameof(KeyDown))
                .Select(ep => ep.EventArgs as KeyEventArgs)
                .Select(e => KeyboardBeatProvider.GetDrumValueForKey(e.Key));

            ViewModel!.StopRecordingCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine(ex.Message));
            ViewModel.WhenAnyValue(vm => vm.CurrentMeasure).Subscribe(measure =>
            {
                MeasuresViewer.BringCurrentMeasureIntoView(measure);
            });
            // Observable.StartAsync(ViewModel.LoadSheets, RxApp.TaskpoolScheduler)
            //     .ObserveOn(RxApp.MainThreadScheduler)
            //     .Subscribe(); // truly fire-and-forget
            _ = ViewModel.LoadSheets();
        });
    }

    // private ItemsControl MeasureControl => this.FindControl<ItemsControl>("MeasuresItemControl")!;
    private MeasureView MeasureView => this.FindControl<MeasureView>("measure")!;
    private Button _startRecordingButton => this.FindControl<Button>("StartRecordingButton")!;
    private Button _stopRecordingButton => this.FindControl<Button>("StopRecordingButton")!;
    private Button _pauseRecordingButton => this.FindControl<Button>("PauseRecordingButton")!;
    private Button _resumeRecordingButton => this.FindControl<Button>("ResumeRecordingButton")!;
    private NumericUpDown _bpmNumeric => this.FindControl<NumericUpDown>("BpmNumeric")!;
    private TextBlock _timeElapsedTB => this.FindControl<TextBlock>("TimeElapsedTextBlock")!;
    private TextBlock _countDownTB => this.FindControl<TextBlock>("CountdownTextBlock")!;
    private Grid _countDownGrid => this.FindControl<Grid>("CountdownGrid")!;
    private CheckBox _keyboardCheckBox => this.FindControl<CheckBox>("KeyboardInputCheckBox")!;

    private async Task SaveHandler(IInteractionContext<SheetCreationData, SheetNameAndDescription> context)
    {
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var saveView = new SaveSheetView { ViewModel = new SaveSheetViewModel(context.Input) };
        var result = await saveView.ShowDialog<SheetNameAndDescription>(mainWindow);
        context.SetOutput(result);
    }
}