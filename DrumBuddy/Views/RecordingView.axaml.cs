using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels;
using DrumBuddy.Views.HelperViews;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace DrumBuddy.Views;

public partial class RecordingView : ReactiveUserControl<RecordingViewModel>
{
    private ItemsControl MeasureControl => this.FindControl<ItemsControl>("MeasuresItemControl");
    private MeasureView MeasureView => this.FindControl<MeasureView>("measure");
    private Button _startRecordingButton => this.FindControl<Button>("StartRecordingButton");
    private Button _stopRecordingButton => this.FindControl<Button>("StopRecordingButton");
    private Button _pauseRecordingButton => this.FindControl<Button>("PauseRecordingButton");
    private Button _resumeRecordingButton => this.FindControl<Button>("ResumeRecordingButton");
    private NumericUpDown _bpmNumeric => this.FindControl<NumericUpDown>("BpmNumeric");
    private TextBlock _timeElapsedTB => this.FindControl<TextBlock>("TimeElapsedTextBlock");
    public RecordingView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            //MeasureView.ViewModel= new MeasureViewModel();
            // ViewModel.BeatObservableFromUI = Observable.FromEventPattern<RoutedEventArgs>(SnareButton, nameof(SnareButton.Click))
            //     .Select(_ => new Beat(DateTime.Now, DrumType.Snare));

            this.OneWayBind(ViewModel, vm => vm.Measures, v => v.MeasureControl.ItemsSource)
                .DisposeWith(d);
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
                i =>
                {
                    return !i;
                });
            this.OneWayBind(ViewModel, vm => vm.IsRecording, v => v._stopRecordingButton.IsVisible,
                i =>
                {
                    return i;
                });
            ViewModel.WhenAnyValue(vm => vm.IsRecording, vm => vm.IsPaused)
                .Subscribe(rp =>
                {
                    if (rp.Item1 && !rp.Item2)
                    {
                        _pauseRecordingButton.IsVisible = true;
                    }
                    else
                    {
                        _pauseRecordingButton.IsVisible = false;
                    }
                })
                .DisposeWith(d);
            ViewModel.WhenAnyValue(vm => vm.IsRecording, vm => vm.IsPaused)
                .Subscribe(rp =>
                {
                    if (rp.Item1 && rp.Item2)
                    {
                        _resumeRecordingButton.IsVisible = true;
                    }
                    else
                    {
                        _resumeRecordingButton.IsVisible = false;
                    }
                })
                .DisposeWith(d);
            this.Bind(ViewModel, vm => vm.BpmDecimal, v => v._bpmNumeric.Value);
            this.Bind(ViewModel, vm => vm.TimeElapsed, v => v._timeElapsedTB.Text);
            // ViewModel.NoteObservable.ObserveOn(RxApp.MainThreadScheduler).Subscribe(notes =>
            // {
            //     foreach (var note in notes)
            //     {
            //         Output.Text = Output.Text += $"A {note.DrumType.ToString()} with {note.Value} value was hit. Timing: {note.Timing}\n";
            //     }
            // });
        });
        AvaloniaXamlLoader.Load(this);
    }
}