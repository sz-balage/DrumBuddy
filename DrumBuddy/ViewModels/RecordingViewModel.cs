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

namespace DrumBuddy.ViewModels
{
    public partial class RecordingViewModel : ReactiveObject, IRoutableViewModel
    {
        private SourceList<MeasureViewModel> _measureSource = new();
        private RecordingService _recordingService;
        private ReadOnlyObservableCollection<MeasureViewModel> _measures;
        
        private DispatcherTimer _timer;

        private IDisposable _recordingSubscription;
        private BPM _bpm;

        public RecordingViewModel()
        {
            _recordingService = new();
            HostScreen = Locator.Current.GetService<IScreen>();
            _measureSource.Connect()
                .Bind(out _measures)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe();
            _measureSource.AddRange(Enumerable.Range(1, 70).ToList().Select(i => new MeasureViewModel()));
            
            this.WhenAnyValue(vm => vm.BpmDecimal) //when the bpm changes, update the _bpm prop (should never be invalid due to the NumericUpDown control)
                .Subscribe(i =>
                {
                    BPM.From(Convert.ToInt32(i)).Match(
                        Right: bpm => _bpm = bpm,
                        Left: ex => Debug.WriteLine(ex.Message));
                });
            BpmDecimal = 100; //default value
            TimeElapsed = "0:0:0";
            IsRecording = false;
            IsPaused = false;
        }
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
            _timer = new DispatcherTimer();
            _timer.Tick += (s, e) =>
                TimeElapsed = $"{_recordingService.StopWatch.Elapsed.Minutes}:{_recordingService.StopWatch.Elapsed.Seconds}:{_recordingService.StopWatch.Elapsed.Milliseconds.ToString().Remove(1)}";
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            IsRecording = true;
            IsPaused = false;
            _recordingSubscription = _recordingService.StartRecording(notes => notes.ToList().ForEach(n => Debug.WriteLine($"{n.Value.ToString()} was hit with {n.DrumType.ToString()}")), _bpm);
            _timer.Start();
        }

        [ReactiveCommand]
        private void StopRecording()
        {
            IsRecording = false;
            IsPaused = false;
            _recordingSubscription.Dispose();
            _timer.Stop();
            _recordingService.StopRecording();
            //do something with the done sheet
            TimeElapsed = $"0:0:0";
        }

        [ReactiveCommand]
        private void PauseRecording()
        {
            IsPaused = true;
            _recordingSubscription.Dispose();
            _recordingService.PauseRecording();
            _timer.Stop();
        }
        [ReactiveCommand]
        private void ResumeRecording()
        {
            IsPaused = false;
            _recordingSubscription = _recordingService.StartRecording(notes => notes.ToList().ForEach(n => Debug.WriteLine($"{n.Value.ToString()} was hit with {n.DrumType.ToString()}")), _bpm);
            _timer.Start();
        }
        public ReadOnlyObservableCollection<MeasureViewModel> Measures => _measures;
        public string? UrlPathSegment { get; } = "recording-view";
        public IScreen HostScreen { get; }
    }
}
