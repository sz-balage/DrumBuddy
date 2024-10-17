using DrumBuddy.Core.Services;
using DrumBuddy.IO.Models;
using DrumBuddy.ViewModels.HelperViewModels;
using DynamicData;
using ReactiveUI;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Reactive.Linq;
using Avalonia.Threading;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.ViewModels
{
    public partial class RecordingViewModel : ReactiveObject, IRoutableViewModel
    {
        private DispatcherTimer _pointerTimer;
        private RecordingService _recordingService;
        private SourceList<MeasureViewModel> _measureSource = new();
        private ReadOnlyObservableCollection<MeasureViewModel> _measures;
        private DispatcherTimer _timer;
        private BPM _bpm;
        private IDisposable _pointerSubscription;
        private IObservable<bool> _stopRecordingCanExecute;
        private SoundPlayer _normalBeepPlayer;
        private SoundPlayer _highBeepPlayer;
        public RecordingViewModel()
        {
            _recordingService = new();
            HostScreen = Locator.Current.GetService<IScreen>();
            //init sound players
            _normalBeepPlayer = new SoundPlayer(@"C:\Users\PC\Source\Repos\baluka1118\DrumBuddy\DrumBuddy\Assets\metronome.wav"); //relative path should be used
            _highBeepPlayer = new SoundPlayer(@"C:\Users\PC\Source\Repos\baluka1118\DrumBuddy\DrumBuddy\Assets\metronomeup.wav");
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
        [Reactive]
        public MeasureViewModel _currentMeasure;
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
        private IDisposable _countDownSubscription;

        [ReactiveCommand]
        private void StartRecording()
        {
            #region UI timer init
            _timer = new DispatcherTimer();
            _timer.Tick += (s, e) =>
                TimeElapsed = $"{_recordingService.StopWatch.Elapsed.Minutes}:{_recordingService.StopWatch.Elapsed.Seconds}:{_recordingService.StopWatch.Elapsed.Milliseconds.ToString().Remove(1)}";
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            #endregion
            _timer.Start(); //should be automatically started when _recordingService.StopWatch.Start() is called (and stop as well)
            var metronomeObs = _recordingService.GetMetronomeBeeping(_bpm);
            CountDown = 5;

            _countDownSubscription = metronomeObs 
                .Take(4)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(HandleCountDown);
            _pointerSubscription = metronomeObs
                .Skip(4)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(MovePointerOnMetronomeBeeps);
            #region UI buttons
            IsRecording = true;
            IsPaused = false;
            #endregion
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


        [ReactiveCommand(CanExecute = nameof(_stopRecordingCanExecute))]
        private void StopRecording()
        {
            var measures = Measures.Where(m => !m.IsEmpty).Select(vm => vm.Measure).ToList();
            _recordingService.StopRecording(_bpm,measures);
            _pointerSubscription.Dispose(); //composite disposable should be introduced
            _timer.Stop();
            StopAndResetPointer();
            //do something with the done sheet
            IsRecording = false;
            IsPaused = false;
            TimeElapsed = $"0:0:0";
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

        private void StopAndResetPointer()
        {
            _pointerSubscription.Dispose();
            ClearMeasures();
            CurrentMeasure.IsPointerVisible = false;
            CurrentMeasure = null;
        }
        public ReadOnlyObservableCollection<MeasureViewModel> Measures => _measures;
        public string? UrlPathSegment { get; } = "recording-view";
        public IScreen HostScreen { get; }
    }
}
