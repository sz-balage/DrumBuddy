using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Enums;
using DrumBuddy.IO.Models;
using DrumBuddy.IO.Extensions;
using System.Diagnostics;
using System.Reactive.Linq;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.IO.Services;

namespace DrumBuddy.Core.Services
{
    public class RecordingService
    {
        private IMidiService _midiService;
        public List<Sheet> Sheets { get; private set; } = new();
        public RecordingService(IMidiService midiService = null)
        {
            _midiService = midiService ?? new MidiService();
        }
        private IList<Note> GenerateNotesFromBeats(IList<(Beat b, long ElapsedMilliseconds)> beats) => beats.Count() switch
        {
            0 => new List<Note>() { new(DrumType.Rest, NoteValue.Quarter, Timing.Perfect) },
            1 => new List<Note> { new(beats.First().b.Drum, NoteValue.Quarter, Timing.Perfect) },
            _ => new List<Note>()
        };
        public IObservable<IList<Note>> GetNotesObservable(IObservable<Beat> BeatsObservable) =>
            BeatsObservable
                        .Select(b => (b, StopWatch.ElapsedMilliseconds))
                        .Buffer(Tempo.QuarterNoteDuration())
                        .Select(GenerateNotesFromBeats);

        public IDisposable StartRecording(Action<IList<Note>> subToNotes, BPM tempo)
        {
            Tempo = tempo;
            StopWatch.Start();
            return GetNotesObservable(_midiService.GetBeatsObservable())
                .Subscribe(subToNotes);
        }

        public void StopRecording(BPM bpm,List<Measure> measures)
        {
            StopWatch.Reset();
            Sheets.Add(new(bpm,measures));
        }
        public Stopwatch StopWatch { get; } = new();
        public BPM Tempo { get;  set; }

        public void PauseRecording() => StopWatch.Stop();
    }
}
