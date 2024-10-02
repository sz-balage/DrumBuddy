using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Enums;
using DrumBuddy.IO.Models;
using DrumBuddy.IO.Extensions;
using System.Diagnostics;
using System.Reactive.Linq;

namespace DrumBuddy.Core.Services
{
    public class RecordingService
    {
        internal Stopwatch _stopwatch = new();
        private BPM _tempo;
        public RecordingService(BPM tempo)
        {
            _tempo = tempo;
        }
        internal void ResetWatch() => _stopwatch.Restart();
        public IObservable<IList<Note>> GetNotesObservable(IObservable<Beat> BeatsObservable) =>
            BeatsObservable
                        .Select(b => (b, _stopwatch.ElapsedMilliseconds))
                        .Buffer(_tempo.QuarterNoteDuration())
                        .Select(GenerateNotesFromBeats);

        private IList<Note> GenerateNotesFromBeats(IList<(Beat b, long ElapsedMilliseconds)> beats) => beats.Count() switch
        {
            0 => new List<Note>() { new(DrumType.Rest, NoteValue.Quarter, Timing.Perfect) },
            1 => new List<Note> { new(beats.First().b.Drum, NoteValue.Quarter, Timing.Perfect) },
            _ => new List<Note>()
        };
    }
}
