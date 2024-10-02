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
using System.Linq;
using System.Reactive.Linq;

namespace DrumBuddy.ViewModels
{
    public class RecordingViewModel : ReactiveObject, IRoutableViewModel
    {
        private SourceList<MeasureViewModel> _measureSource = new();
        private IObservable<Beat> _beatObservableFromUI;
        private RecordingService _recordingService;
        private ReadOnlyObservableCollection<MeasureViewModel> _measures;
        public RecordingViewModel(IScreen host = null)
        {

            HostScreen = host ?? Locator.Current.GetService<IScreen>();
            var bpm = BPM.From(100);
            bpm.Match(
                Right: b => _recordingService = new(b),
                Left: e => { });
            _measureSource.Connect()
                .Bind(out _measures)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe();
            _measureSource.AddRange(Enumerable.Range(1, 70).ToList().Select(i => new MeasureViewModel()));
        }

        public ReadOnlyObservableCollection<MeasureViewModel> Measures => _measures;
        public IObservable<Beat> BeatObservableFromUI
        {
            get => _beatObservableFromUI;
            set => this.RaiseAndSetIfChanged(ref _beatObservableFromUI, value);
        }
        public IObservable<IList<Note>> NoteObservable =>
            _recordingService.GetNotesObservable(BeatObservableFromUI);

        public string? UrlPathSegment { get; } = "recording-view";
        public IScreen HostScreen { get; }
    }
}
