using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using DrumBuddy.Core.Extensions;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.Extensions;
using DrumBuddy.IO;
using DrumBuddy.IO.Services;
using DrumBuddy.Models;
using DrumBuddy.Services;
using DrumBuddy.ViewModels.HelperViewModels;
using DrumBuddy.Views.HelperViews;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels.Dialogs;

public partial class EditingViewModel : ReactiveObject
{
    private static int _firstMeasurePassedCount;
    private static int _globalPointerIdx;
    private readonly ConfigurationService _configService;
    private readonly LibraryViewModel _library;
    private readonly ReadOnlyObservableCollection<MeasureViewModel> _measures;
    private readonly SourceList<MeasureViewModel> _measureSource = new();
    private readonly MetronomePlayer _metronomePlayer;
    private readonly MidiService _midiService;
    private NotificationService _notificationService;
    private readonly PdfGenerator _pdfGenerator;
    private readonly IObservable<bool> _stopRecordingCanExecute;
    public readonly Sheet OriginalSheet;
    private Bpm _bpm;
    [Reactive] private decimal _bpmDecimal;
    [Reactive] private bool _canSave;

    [Reactive] private int _countDown;
    [Reactive] private bool _countDownVisibility;
    [Reactive] private MeasureViewModel _currentMeasure;

    [Reactive] private bool _isExporting;

    [Reactive] private bool _isRecording;
    [Reactive] private bool _isViewOnly;

    private bool _recordingJustStarted = true;
    private int _selectedEntryPointMeasureIndex;
    private CompositeDisposable _subs = new();
    private long _tick;

    [Reactive] private string _timeElapsed;
    private DispatcherTimer _timer;
    public EditingViewModel(Sheet originalSheet,
        MidiService midiService,
        ConfigurationService configService,
        PdfGenerator pdfGenerator,
        bool isViewOnly = false)
    {
        OriginalSheet = originalSheet;
        _midiService = midiService;
        _configService = configService;
        _pdfGenerator = pdfGenerator;
        IsViewOnly = isViewOnly;
        //init sound players
        _metronomePlayer = Locator.Current.GetRequiredService<MetronomePlayer>();
        //binding measuresource
        _measureSource.Connect()
            .Bind(out _measures)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe();
        _measureSource.AddRange(originalSheet.Measures.Select(m => new MeasureViewModel(m)));
        _measureSource.AddRange(Enumerable.Range(1, 70).ToList().Select(i => new MeasureViewModel()));
        _stopRecordingCanExecute = this.WhenAnyValue(vm => vm.IsRecording, vm => vm.CurrentMeasure,
            (recording, currentMeasure) => recording && currentMeasure != null);
        //bpm changes should update the _bpm prop
        this
            .WhenAnyValue(vm =>
                vm.BpmDecimal) //when the bpm changes, update the _bpm prop (should never be invalid due to the NumericUpDown control)
            .Skip(1)
            .Subscribe(i =>
            {
                var value = Convert.ToInt32(i);
                _bpm = new Bpm(value);
            });
        //default values
        BpmDecimal = originalSheet.Tempo;
        TimeElapsed = "0:0:0";
        IsRecording = false;
        HandleMeasureClick(0); //put pointer to first measure by default
        CanSave = false;

        this.WhenAnyValue(vm => vm.IsRecording)
            .Subscribe(recording => CanSave = !recording);
    }

    public void SetTopLevelWindow(Window window)
    {
        _notificationService = new(window);
    }
    private bool _keyboardInputEnabled => _configService.KeyboardInput;

    public IObservable<int> KeyboardBeats { get; set; }
    public ReadOnlyObservableCollection<MeasureViewModel> Measures => _measures;

    public IObservable<bool> CanNavigate => this.WhenAnyValue(
        vm => vm.IsRecording,
        recording => !recording);

    public async Task ExportSheetToPdfAsync(List<MeasureView> measures)
    {
        IsExporting = true;
        try
        {
            await _pdfGenerator.ExportSheetToPdf(measures, OriginalSheet.Name, OriginalSheet.Description,
                OriginalSheet.Tempo);
            var message = $"{OriginalSheet.Name} exported successfully.";
            _notificationService.ShowNotification(new Notification("Successful export.", message));
        }
        catch (Exception e)
        {
            _notificationService.ShowNotification(new Notification("Export error.","Error while exporting sheet: " + e.Message,NotificationType.Error));
        }
        finally
        {
            IsExporting = false;
        }
    }

    private void InitTimer()
    {
        _tick = 0;
        _subs = new CompositeDisposable();
        _timer = new DispatcherTimer();
        _timer.Tick += (s, e) =>
        {
            _tick++; //increments every second
            TimeElapsed = $"{_tick / 6000 % 60:D2}:{_tick / 100 % 60:D2}:{_tick % 100:D2}";
        };
        _timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
    }

    private void InitMetronomeSubs()
    {
        var metronomeObs = RecordingService.GetMetronomeBeeping(_bpm);
        _subs.Add(metronomeObs
            .Take(5)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(HandleCountDown));
        _subs.Add(metronomeObs
            .Skip(4)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(MovePointerOnMetronomeBeeps));
    }

    private void InitBeatSub()
    {
        // Get the starting measure index
        var startMeasureIdx = CurrentMeasure != null ? Measures.IndexOf(CurrentMeasure) : 0;

        // drum sub
        var measureIdx =
            startMeasureIdx - 1; // Start one measure before so the first increment puts us at the right position
        var rythmicGroupIndex = -1;
        var delay = 5 * _bpm.QuarterNoteDuration() - _bpm.SixteenthNoteDuration() / 2.0
                    + _bpm
                        .SixteenthNoteDuration(); //5 times the quarter because of how observable.interval works (first wait the interval, only then starts emitting)

        var tempNotes = new List<NoteGroup>();
        _subs.Add(RecordingService
            .GetNotes(_bpm,
                _keyboardInputEnabled
                    ? KeyboardBeats.GetMappedBeatsForKeyboard(_configService)
                    : _midiService.GetMappedBeatsObservable(_configService))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select((notes, idx) => (notes, idx))
            .DelaySubscription(delay)
            .Subscribe(data =>
            {
                var absoluteIdx = data.idx;
                var localMIdx = startMeasureIdx + absoluteIdx / 16;
                var localRgIdx = absoluteIdx % 16 / 4;

                if (measureIdx != localMIdx || rythmicGroupIndex != localRgIdx)
                    if (rythmicGroupIndex != localRgIdx || measureIdx != localMIdx)
                    {
                        if (tempNotes.Count > 0)
                        {
                            if (measureIdx >= 0 && measureIdx < Measures.Count)
                                Measures[measureIdx].AddRythmicGroupFromNotes(tempNotes, rythmicGroupIndex);
                            tempNotes.Clear();
                        }

                        measureIdx = localMIdx;
                        rythmicGroupIndex = localRgIdx;
                    }

                tempNotes.Add(new NoteGroup(data.notes));
            }));
    }

    private void HandleCountDown(long idx)
    {
        if (idx == 5)
            return;
        if (idx == 0)
        {
            CountDownVisibility = true;
            _metronomePlayer.PlayHighBeep();
        }
        else
        {
            _metronomePlayer.PlayNormalBeep();
        }

        CountDown--;
    }

    private void MovePointerOnMetronomeBeeps(long idx)
    {
        if (idx == 0)
        {
            _metronomePlayer.PlayHighBeep();
            if (CurrentMeasure == Measures[_selectedEntryPointMeasureIndex] && _firstMeasurePassedCount == 0)
            {
                CountDownVisibility = false;
                _firstMeasurePassedCount++;
            }
            else
            {
                CurrentMeasure.IsPointerVisible = false;
                CurrentMeasure = Measures[Measures.IndexOf(CurrentMeasure) + 1];
                CurrentMeasure.IsPointerVisible = true;
            }
        }
        else
        {
            _metronomePlayer.PlayNormalBeep();
        }

        CurrentMeasure?.MovePointerToRg(idx);
        if (_globalPointerIdx == 0)
            _timer.Start();
        _globalPointerIdx++;
    }

    public void HandleMeasureClick(int measureIndex)
    {
        if (IsRecording || IsViewOnly)
            return;

        if (CurrentMeasure != null!)
            CurrentMeasure.IsPointerVisible = false;

        _selectedEntryPointMeasureIndex = measureIndex;
        CurrentMeasure = Measures[_selectedEntryPointMeasureIndex];
        CurrentMeasure.IsPointerVisible = true;
        CurrentMeasure.MovePointerToRg(0);
    }

    [ReactiveCommand]
    private void StartRecording()
    {
        // Make sure a measure is selected
        InitTimer();
        CountDown = 5;
        InitMetronomeSubs();
        InitBeatSub();
        _globalPointerIdx = 0;

        IsRecording = true;
    }

    public Sheet Save()
    {
        var measures = Measures.Where(m => !m.IsEmpty).Select(vm => vm.Measure).ToImmutableArray();
        return new Sheet(_bpm, measures, OriginalSheet.Name, OriginalSheet.Description);
    }

    [ReactiveCommand(CanExecute = nameof(_stopRecordingCanExecute))]
    private void StopRecording()
    {
        _subs.Dispose();
        _timer.Stop();
        ResetPointer();
        IsRecording = false;
        TimeElapsed = "0:0:0";
        _firstMeasurePassedCount = 0;

        var recordedCount = Measures.Count(m => !m.IsEmpty);
        if (recordedCount > 0)
            _notificationService.ShowNotification(new Notification("Recording stopped.",$"Captured {recordedCount} measure(s)."));
        else
            _notificationService.ShowNotification(new Notification("Recording stopped.","No notes captured.", NotificationType.Warning));
    }

    private void ResetPointer()
    {
        CurrentMeasure.IsPointerVisible = false;
        if (CurrentMeasure != Measures.Last())
        {
            var nextIndex = Measures.IndexOf(CurrentMeasure) + 1;
            CurrentMeasure = Measures[nextIndex];
            _selectedEntryPointMeasureIndex = nextIndex;
            CurrentMeasure.IsPointerVisible = true;
            CurrentMeasure.MovePointerToRg(0);
        }
    }
}