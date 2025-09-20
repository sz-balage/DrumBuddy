using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Threading;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Client.ViewModels.HelperViewModels;
using DrumBuddy.Core.Extensions;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.IO.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.Client.ViewModels.Dialogs;

public partial class EditingViewModel : ReactiveObject
{
    private readonly SoundPlayer _highBeepPlayer;
    private readonly LibraryViewModel _library;
    private readonly ReadOnlyObservableCollection<MeasureViewModel> _measures;
    private readonly SourceList<MeasureViewModel> _measureSource = new();
    private readonly IMidiService _midiService;
    private readonly ConfigurationService _configService;
    private readonly SoundPlayer _normalBeepPlayer;
    private Bpm _bpm;
    [Reactive] private decimal _bpmDecimal;

    [Reactive] private int _countDown;
    [Reactive] private bool _countDownVisibility;
    [Reactive] private bool _isViewOnly;
    [Reactive] private MeasureViewModel _currentMeasure;
    private int _selectedEntryPointMeasureIndex;

    [Reactive] private bool _isRecording;

    // Add new properties
    [Reactive] private bool _canSave;
    private bool _keyboardInputEnabled => _configService.IsKeyboardEnabled;
    private IObservable<bool> _stopRecordingCanExecute;
    private CompositeDisposable _subs = new();
    private long _tick;

    [Reactive] private string _timeElapsed;
    private DispatcherTimer _timer;
    private readonly Sheet _originalSheet;

    public EditingViewModel(Sheet originalSheet, bool isViewOnly = false)
    {
        _originalSheet = originalSheet;
        _midiService = Locator.Current.GetRequiredService<IMidiService>();
        _configService = Locator.Current.GetRequiredService<ConfigurationService>();
        IsViewOnly = isViewOnly;
        //init sound players
        _normalBeepPlayer =
            new SoundPlayer(FileSystemService.GetPathToRegularBeepSound()); //relative path should be used
        _highBeepPlayer = new SoundPlayer(FileSystemService.GetPathToHighBeepSound());
        _normalBeepPlayer.Load();
        _highBeepPlayer.Load();
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

    public IObservable<int> KeyboardBeats { get; set; }
    public ReadOnlyObservableCollection<MeasureViewModel> Measures => _measures;
    public string? UrlPathSegment { get; } = "recording-view";
    public IScreen HostScreen { get; }

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
        var delay = 5 * _bpm.QuarterNoteDuration() -
                    _bpm.SixteenthNoteDuration() / 2.0;

        var tempNotes = new List<NoteGroup>();
        _subs.Add(RecordingService
            .GetNotes(_bpm, _keyboardInputEnabled ? KeyboardBeats.GetMappedBeatsForKeyboard(_configService) : _midiService.GetMappedBeatsObservable(_configService))
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
            _highBeepPlayer.Play();
        }
        else
        {
            _normalBeepPlayer.Play();
        }

        CountDown--;
    }

    private bool _recordingJustStarted = true;
    private static int _firstMeasurePassedCount = 0;
    private void MovePointerOnMetronomeBeeps(long idx)
    {
        if (idx == 0)
        {
            _highBeepPlayer.Play();
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
            _normalBeepPlayer.Play();
        }

        CurrentMeasure?.MovePointerToRg(idx);
    }

    public void HandleMeasureClick(int measureIndex)
    {
        if (IsRecording)
            return;

        // Reset any existing pointer
        if (CurrentMeasure != null!)
            CurrentMeasure.IsPointerVisible = false;

        // Set the new current measure
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

        IsRecording = true;
    }
    public IObservable<bool> CanNavigate => this.WhenAnyValue(
        vm => vm.IsRecording,
        recording => !recording);
    public Sheet Save()
    {
        var measures = Measures.Where(m => !m.IsEmpty).Select(vm => vm.Measure).ToImmutableArray();
        return new Sheet(_bpm, measures, _originalSheet.Name, _originalSheet.Description);
    }

    [ReactiveCommand(CanExecute = nameof(_stopRecordingCanExecute))]
    private void StopRecording()
    {
        _subs.Dispose();
        _timer.Stop();
        ResetPointer();
        //do something with the done sheet
        IsRecording = false;
        TimeElapsed = "0:0:0";
        _firstMeasurePassedCount = 0;
    }

    private void ClearMeasures()
    {
        _measureSource.Clear();
        _measureSource.AddRange(Enumerable.Range(1, 70).ToList().Select(i => new MeasureViewModel()));
    }

    private void ResetPointer()
    {
        CurrentMeasure.IsPointerVisible = false;
        if (CurrentMeasure != Measures.Last())
        {
            int nextIndex = Measures.IndexOf(CurrentMeasure) + 1;
            CurrentMeasure = Measures[nextIndex];
            _selectedEntryPointMeasureIndex = nextIndex;
            CurrentMeasure.IsPointerVisible = true;
            CurrentMeasure.MovePointerToRg(0);
        }
    }
}