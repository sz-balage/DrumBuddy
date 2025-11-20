using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using DrumBuddy.DesignHelpers;
using DrumBuddy.Models;
using DrumBuddy.Services;
using DrumBuddy.ViewModels.Dialogs;
using ReactiveUI;
using Splat;

namespace DrumBuddy.Views.Dialogs;

public partial class EditingView : ReactiveWindow<EditingViewModel>
{
    public EditingView()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
            ViewModel = new EditingViewModel(TestSheetProvider.GetTestSheet(), null, null, null, true);
        this.WhenActivated(d =>
        {
            // this.OneWayBind(ViewModel, vm => vm.Measures, v => v.MeasureControl.ItemsSource)
            //     .DisposeWith(d);
            ViewModel.SetTopLevelWindow(this);
            this.OneWayBind(ViewModel, vm => vm.IsExporting, v => v.ExportTextBlock.Text, isExporting =>
                    isExporting ? "Exporting..." : "Export to pdf")
                .DisposeWith(d);
            Title = "Viewing Sheet - " + ViewModel.OriginalSheet.Name;
            this.Bind(ViewModel, vm => vm.BpmDecimal, v => v._bpmNumeric.Value);
            //editing stuff
            if (!ViewModel.IsViewOnly)
            {
                Title = "Editing Sheet - " + ViewModel.OriginalSheet.Name;
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
                this.Bind(ViewModel, vm => vm.TimeElapsed, v => v._timeElapsedTB.Text);
                this.OneWayBind(ViewModel, vm => vm.CountDown, v => v._countDownTB.Text)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CountDownVisibility, v => v._countDownGrid.IsVisible)
                    .DisposeWith(d);

                ViewModel.KeyboardBeats = Observable.FromEventPattern(this, nameof(KeyDown))
                    .Select(ep => ep.EventArgs as KeyEventArgs)
                    .Select(e => KeyboardBeatProvider.GetDrumValueForKey(e.Key));
                ViewModel!.StopRecordingCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine(ex.Message));
                ViewModel.WhenAnyValue(vm => vm.CurrentMeasure).Subscribe(measure =>
                {
                    MeasuresViewer.BringCurrentMeasureIntoView(measure);
                });
                MeasuresViewer.MeasurePressedIdx.Subscribe(idx => ViewModel.HandleMeasureClick(idx)).DisposeWith(d);
            }
        });
    }

    // private ItemsControl MeasureControl => this.FindControl<ItemsControl>("MeasuresItemControl")!;
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

    private void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var result = ViewModel.Save();
        Close(result);
    }

    private async void Export_OnClick(object? sender, RoutedEventArgs e)
    {
        var measures = MeasuresViewer.GetVisualDescendants();
        await ViewModel.ExportSheetToPdfAsync(measures);
    }
}