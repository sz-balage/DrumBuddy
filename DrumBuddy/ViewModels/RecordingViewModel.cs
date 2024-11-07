using DrumBuddy.Core.Services;
using DrumBuddy.IO.Models;
using DrumBuddy.ViewModels.HelperViewModels;
using DynamicData;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI.SourceGenerators;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Extensions;
using DrumBuddy.IO.Services;
using LanguageExt;
using Unit = System.Reactive.Unit;

namespace DrumBuddy.ViewModels
{
    public partial class RecordingViewModel : ReactiveObject, IRoutableViewModel
    { 
        private readonly RecordingService _recordingService;
        private readonly SourceList<MeasureViewModel> _measureSource = new();
        private readonly ReadOnlyObservableCollection<MeasureViewModel> _measures;
        private DispatcherTimer _timer;
        private BPM _bpm;
        private IObservable<bool> _stopRecordingCanExecute;
        private readonly SoundPlayer _normalBeepPlayer;
        private readonly SoundPlayer _highBeepPlayer;
        private readonly LibraryViewModel _library;
        private CompositeDisposable _subs = new();
        private long _tick;
        public RecordingViewModel()
        {
            _recordingService = new();
            HostScreen = Locator.Current.GetService<IScreen>();
            _library = Locator.Current.GetService<LibraryViewModel>();
            //init sound players
            _normalBeepPlayer = new SoundPlayer(FileSystemService.GetPathToRegularBeepSound()); //relative path should be used
            _highBeepPlayer = new SoundPlayer(FileSystemService.GetPathToHighBeepSound());
            _normalBeepPlayer.Load();
            _highBeepPlayer.Load();
            //binding measuresource
            _measureSource.Connect()
                .Bind(out _measures)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe();
            _measureSource.AddRange(Enumerable.Range(1, 70).ToList().Select(i => new MeasureViewModel()));
            _stopRecordingCanExecute = this.WhenAnyValue(vm => vm.IsRecording, vm => vm.CurrentMeasure, (recording, currentMeasure) => recording && currentMeasure != null);
            //bpm changes should update the _bpm prop
            this.WhenAnyValue(vm => vm.BpmDecimal) //when the bpm changes, update the _bpm prop (should never be invalid due to the NumericUpDown control)
                .Subscribe(i =>
                {
                    BPM.From(Convert.ToInt32(i)).Match(
                        Right: bpm => _bpm = bpm,
                        Left: ex => Debug.WriteLine(ex.Message));
                });
            //default values
            BpmDecimal = 100; 
            TimeElapsed = "0:0:0";
            IsRecording = false;
            IsPaused = false;
            CurrentMeasure = null;
        }
        private void InitTimer()
        {
            _tick = 0;
            _subs = new CompositeDisposable();
            _timer = new DispatcherTimer();
            _timer.Tick += (s, e) =>
            {
                _tick++; //increments every second
                TimeElapsed = $"{(_tick / 6000) % 60:D2}:{(_tick / 100) % 60:D2}:{_tick % 100:D2}";
                
            };
            _timer.Interval = new TimeSpan(0, 0, 0, 0,10);
        }
        private void InitMetronomeSubs()
        {
            var metronomeObs = _recordingService.GetMetronomeBeeping(_bpm);
            _subs.Add(metronomeObs 
                .Take(4)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(onNext: HandleCountDown, onCompleted: () => _timer.Start()));
            _subs.Add(metronomeObs
                .Skip(4)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(MovePointerOnMetronomeBeeps));
        }
        private void InitBeatSub()
        {
            //beat sub
            var measureIdx = -1;
            var rythmicGroupIndex = -1;
            var delay  = (5*_bpm.QuarterNoteDuration()) - (_bpm.SixteenthNoteDuration() / 2.0); //5 times the quarter because of how observable.interval works (first wait the interval, only then starts emitting)
            var tempNotes = new List<Note>();
            _subs.Add(_recordingService.GetNotes(_bpm)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select((notes, idx) => (notes, idx))
                .DelaySubscription(delay)
                .Subscribe(data =>
                {
                    //idx is the current sixteenth note 
                    var localMIdx = data.idx / 16;
                    var localRgIdx = (data.idx % 16) / 4; 
                    measureIdx = localMIdx;
                    if(rythmicGroupIndex != localRgIdx)
                    {
                        rythmicGroupIndex = localRgIdx;
                        if(rythmicGroupIndex == 0)
                        {
                            if(measureIdx != 0)
                                Measures[measureIdx-1].AddRythmicGroupFromNotes(tempNotes);
                        }   
                        else
                        {
                            Measures[measureIdx].AddRythmicGroupFromNotes(tempNotes);
                        }
                        tempNotes.Clear();
                    }
                    tempNotes.Add(data.notes);
                }));
        }
        private void HandleCountDown(long idx)
        {
            if(idx == 0)
            {
                CountDownVisibility = true;
                _highBeepPlayer.Play();
            }
            else
                _normalBeepPlayer.Play();
            CountDown--;
        }
        private void MovePointerOnMetronomeBeeps(long idx)
        {
            if (idx == 0)
            {
                _highBeepPlayer.Play();
                if (CurrentMeasure == null)
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
                _normalBeepPlayer.Play();
            CurrentMeasure?.MovePointerToRG(idx);
        }
        public Interaction<Unit, Option<string>> ShowSaveDialog { get; } = new();
        [Reactive]
        private MeasureViewModel _currentMeasure;
        [Reactive]
        private decimal _bpmDecimal;
        [Reactive]
        private bool _isRecording;
        [Reactive]
        private bool _isPaused;

        [Reactive]
        private string _timeElapsed;

        [Reactive]
        private int _countDown;
        [Reactive]
        private bool _countDownVisibility;
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
            TimeElapsed = $"0:0:0";
            
            var measures = Measures.Where(m => !m.IsEmpty).Select(vm => vm.Measure).ToList();
            //ask user if sheet should be saved
            var sheet = new Sheet(_bpm, measures, "test");
            var save = await ShowSaveDialog.Handle(Unit.Default);
            if(save.IsSome)
                _library.AddSheet(new Sheet(_bpm, measures, (string)save));
            ClearMeasures();
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
        public ReadOnlyObservableCollection<MeasureViewModel> Measures => _measures;
        public string? UrlPathSegment { get; } = "recording-view";
        public IScreen HostScreen { get; }
    }
}
