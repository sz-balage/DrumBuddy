using DrumBuddy.Core.Models;
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
using System.Reactive.Linq;
using Avalonia.Threading;
using ReactiveUI.SourceGenerators;
using System.ComponentModel;
using System.Reactive.Concurrency;
using DrumBuddy.IO.Enums;
using DrumBuddy.IO.Extensions;

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
        public RecordingViewModel()
        {
            _recordingService = new();
            HostScreen = Locator.Current.GetService<IScreen>();
            //binding measuresource
            _measureSource.Connect()
                .Bind(out _measures)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe();
            _measureSource.AddRange(Enumerable.Range(1, 70).ToList().Select(i => new MeasureViewModel()));
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
        private IObservable<IList<Note>> _gettingNotesWhileRecordingObs => 
            _recordingService.GetNotesObservable(Observable.Interval(_bpm.QuarterNoteDuration()).Select(_ => new Beat(DateTime.Now,DrumType.Bass)))
                .DoWhile(() => IsRecording);

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
        [ReactiveCommand]
        private void StartRecording()
        {
            #region UI timer init
            _timer = new DispatcherTimer();
            _timer.Tick += (s, e) =>
                TimeElapsed = $"{_recordingService.StopWatch.Elapsed.Minutes}:{_recordingService.StopWatch.Elapsed.Seconds}:{_recordingService.StopWatch.Elapsed.Milliseconds.ToString().Remove(1)}";
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            #endregion

            #region UI buttons
            IsRecording = true;
            IsPaused = false;
            #endregion

            _timer.Start();
            _recordingService.Tempo = _bpm;
            _recordingService.StopWatch.Start();
            _pointerSubscription = _gettingNotesWhileRecordingObs
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select((notes, i) => (notes, i % 4)) // change the select to include i % 4
                .Subscribe((tuple) =>
                {
                    //go to the next measure every 4 beats
                    if (tuple.Item2 == 0 && CurrentMeasure != Measures.Last())
                    {
                        if (CurrentMeasure == null)
                        {
                            CurrentMeasure = Measures[0];
                        }
                        else
                        {
                            CurrentMeasure.IsPointerVisible = false;
                            CurrentMeasure = Measures[Measures.IndexOf(CurrentMeasure) + 1];
                        }
                    }
                    //add the notes to the current measure and move the pointer
                    CurrentMeasure.MovePointerToNextRythmicGroup(tuple.Item2);
                });
        }

        [ReactiveCommand]
        private void StopRecording()
        {
            IsRecording = false;
            IsPaused = false;
            _timer.Stop();
            _recordingService.StopWatch.Reset();
            StopAndResetPointer();
            //do something with the done sheet
            TimeElapsed = $"0:0:0";
        }

        private void ClearMeasures()
        {
            _measureSource.Clear();
            _measureSource.AddRange(Enumerable.Range(1, 70).ToList().Select(i => new MeasureViewModel()));
        }

        [ReactiveCommand]
        private void PauseRecording()
        {
            IsPaused = true;
            _recordingService.PauseRecording();
            _timer.Stop();
        }
        [ReactiveCommand]
        private void ResumeRecording()
        {
            IsPaused = false;
            _timer.Start();
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
