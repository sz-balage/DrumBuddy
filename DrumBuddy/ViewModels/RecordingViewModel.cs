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
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.ViewModels
{
    public partial class RecordingViewModel : ReactiveObject, IRoutableViewModel
    {
        private SourceList<MeasureViewModel> _measureSource = new();
        private RecordingService _recordingService;
        private ReadOnlyObservableCollection<MeasureViewModel> _measures;
        private IDisposable _recordingSubscription;
        private BPM _bpm;
        [Reactive]
        private decimal _bpmDecimal;
        [Reactive]
        private bool _isRecording;

        [ReactiveCommand]
        private void StartRecording()
        {
            IsRecording = true;
            _recordingSubscription = _recordingService.StartRecording(notes => notes.ToList().ForEach(n => Debug.WriteLine($"{n.Value.ToString()} was hit with {n.DrumType.ToString()}")), _bpm);
        }

        [ReactiveCommand]
        private void StopRecording()
        {
            IsRecording = false;
            _recordingSubscription.Dispose();
            _recordingService.StopRecording();
        }
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
            IsRecording = false;
        }
        public ReadOnlyObservableCollection<MeasureViewModel> Measures => _measures;
        public string? UrlPathSegment { get; } = "recording-view";
        public IScreen HostScreen { get; }
    }
}
