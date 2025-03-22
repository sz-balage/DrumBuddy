using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Client.Models;
using DrumBuddy.Client.ViewModels.HelperViewModels;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Extensions;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.IO.Models;
using DrumBuddy.IO.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using Unit = System.Reactive.Unit;

namespace DrumBuddy.Client.ViewModels;

public partial class RecordingViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly SoundPlayer _highBeepPlayer;
    private readonly LibraryViewModel _library;
    private readonly ReadOnlyObservableCollection<MeasureViewModel> _measures;
    private readonly SourceList<MeasureViewModel> _measureSource = new();
    private readonly IMidiService _midiService;
    private readonly SoundPlayer _normalBeepPlayer;
    private Bpm _bpm;
    [Reactive] private decimal _bpmDecimal;

    [Reactive] private int _countDown;
    [Reactive] private bool _countDownVisibility;
    [Reactive] private MeasureViewModel _currentMeasure;
    [Reactive] private bool _isPaused;
    [Reactive] private bool _isRecording;

    [Reactive] private bool _keyboardInputEnabled;
    private IObservable<bool> _stopRecordingCanExecute;
    private CompositeDisposable _subs = new();
    private long _tick;

    [Reactive] private string _timeElapsed;
    private DispatcherTimer _timer;

    public RecordingViewModel(IScreen hostScreen, IMidiService midiService, LibraryViewModel library)
    {

        HostScreen = hostScreen;
        _midiService = midiService;
        _library = library;
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
        BpmDecimal = 100;
        TimeElapsed = "0:0:0";
        IsRecording = false;
        IsPaused = false;
        CurrentMeasure = null!;

        this.WhenNavigatingFromObservable().Subscribe(_ =>
        {
            _subs.Dispose(); //TODO: dispose disposables when navigating away from the viewmodel
        });
    }

    public IObservable<Drum> KeyboardBeats { get; set; }

    public Interaction<SheetCreationData, string?> ShowSaveDialog { get; } = new();

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
            .Take(4)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(HandleCountDown, () => _timer.Start()));
        _subs.Add(metronomeObs
            .Skip(4)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(MovePointerOnMetronomeBeeps));
    }

    private void InitBeatSub()
    {
        //drum sub
        var measureIdx = -1;
        var rythmicGroupIndex = -1;
        var delay = 5 * _bpm.QuarterNoteDuration() -
                    _bpm.SixteenthNoteDuration() /
                    2.0; //5 times the quarter because of how observable.interval works (first wait the interval, only then starts emitting)
        var tempNotes = new List<NoteGroup>();
        _subs.Add(RecordingService
            .GetNotes(_bpm, KeyboardInputEnabled ? KeyboardBeats : _midiService.GetBeatsObservable())
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select((notes, idx) => (notes, idx))
            .DelaySubscription(delay)
            .Subscribe(data =>
            {
                //idx is the current sixteenth note 
                var localMIdx = data.idx / 16;
                var localRgIdx = data.idx % 16 / 4;
                measureIdx = localMIdx;
                if (rythmicGroupIndex != localRgIdx)
                {
                    rythmicGroupIndex = localRgIdx;
                    if (rythmicGroupIndex == 0)
                    {
                        if (measureIdx != 0)
                            Measures[measureIdx - 1].AddRythmicGroupFromNotes(tempNotes);
                    }
                    else
                    {
                        Measures[measureIdx].AddRythmicGroupFromNotes(tempNotes);
                    }

                    tempNotes.Clear();
                }

                tempNotes.Add(new NoteGroup(data.notes));
            }));
    }

    private void HandleCountDown(long idx)
    {
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

    private void MovePointerOnMetronomeBeeps(long idx)
    {
        if (idx == 0)
        {
            _highBeepPlayer.Play();
            if (CurrentMeasure == null!)
            {
                CountDownVisibility = false;
                CurrentMeasure = Measures[0];
            }
            else
            {
                CurrentMeasure.IsPointerVisible = false;
                CurrentMeasure = Measures[Measures.IndexOf(CurrentMeasure) + 1];
            }
        }
        else
        {
            _normalBeepPlayer.Play();
        }

        CurrentMeasure?.MovePointerToRg(idx);
    }

    [ReactiveCommand]
    private void StartRecording()
    {
        InitTimer();
        CountDown = 5;
        InitMetronomeSubs();
        InitBeatSub();

        #region UI buttons

        IsRecording = true;
        IsPaused = false;

        #endregion
    }


    [ReactiveCommand(CanExecute = nameof(_stopRecordingCanExecute))]
    private async Task StopRecording()
    {
        _subs.Dispose();
        _timer.Stop();
        ResetPointer();
        //do something with the done sheet
        IsRecording = false;
        IsPaused = false;
        TimeElapsed = "0:0:0";

        var measures = Measures.Where(m => !m.IsEmpty).Select(vm => vm.Measure).ToList();
        //ask user if sheet should be saved
        var dialogResult = await ShowSaveDialog.Handle(new SheetCreationData(_bpm, [..measures]));
        // if (save is not null)
        //     await _library.SaveSheet(new Sheet(_bpm, measures, save));
        ClearMeasures();
        if (dialogResult != null)
        {
            var mainVm = HostScreen as MainViewModel;
            mainVm!.NavigateFromCode(Locator.Current.GetRequiredService<LibraryViewModel>());
        }
    }

    private void ClearMeasures()
    {
        _measureSource.Clear();
        _measureSource.AddRange(Enumerable.Range(1, 70).ToList().Select(i => new MeasureViewModel()));
    }

    [ReactiveCommand(CanExecute = nameof(_stopRecordingCanExecute))]
    private void PauseRecording() //not implemented for now
    {
        IsPaused = true;
    }

    [ReactiveCommand]
    private void ResumeRecording() //not implemented for now
    {
        IsPaused = false;
    }

    private void ResetPointer()
    {
        CurrentMeasure.IsPointerVisible = false;
        CurrentMeasure = null;
    }
}