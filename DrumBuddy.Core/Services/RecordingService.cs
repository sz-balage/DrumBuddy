using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Enums;
using DrumBuddy.IO.Models;
using DrumBuddy.IO.Extensions;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.IO.Services;

[assembly: InternalsVisibleTo("DrumBuddy.Core.Unit")]
namespace DrumBuddy.Core.Services
{
    public class RecordingService
    {
        private IMidiService _midiService;
        private IScheduler _scheduler;
        public List<Sheet> Sheets { get; private set; } = new();
        public RecordingService(IMidiService midiService = null, IScheduler scheduler = null)
        {
            _scheduler = scheduler ?? Scheduler.Default;
            _midiService = midiService ?? new MidiService();
        }
        private IList<Note> GenerateNotesFromBeats(IList<(Beat b, long ElapsedMilliseconds)> beats) => beats.Count() switch
        {
            0 => new List<Note>() { new(DrumType.Rest, NoteValue.Quarter, Timing.Perfect) },
            1 => new List<Note> { new(beats.First().b.Drum, NoteValue.Quarter, Timing.Perfect) },
            _ => new List<Note>()
        };
        /// <summary>
        /// Returns the notes in quarter note sequences, based on the incoming midi signals (beats)
        /// </summary>
        /// <returns></returns>
        internal IObservable<IList<Note>> GetNotesObservable() =>
            _midiService.GetBeatsObservable()
                        .Select(b => (b, StopWatch.ElapsedMilliseconds))
                        .Buffer(Tempo.QuarterNoteDuration(),_scheduler)
                        .Select(GenerateNotesFromBeats);

        /// <summary>
        /// Returns a sequence of metronome beeps, and starts the stopwatch.
        /// </summary>
        /// <returns>The index of the current beep relative to the start.</returns>
        public IObservable<long> GetMetronomeBeeping()
        {
            StopWatch.Start();
            return Observable.Interval(Tempo.QuarterNoteDuration(), _scheduler);
        }
            
        /// <summary>
        /// Returns a beat sequence along with its time it was hit relative to the start of the stopwatch buffered by the sixteenth note duration.
        /// </summary>
        /// <returns></returns>
        public IObservable<IList<(Beat,long)>> GetBeatsBuffered() =>
            _midiService.GetBeatsObservable()
                        .Select(b =>
                        {
                            return (b, StopWatch.ElapsedMilliseconds);
                        })
                        .Buffer(Tempo.SixteenthNoteDuration(), _scheduler);





            


        /// <summary>
        /// Starts the recording of incoming notes
        /// </summary>
        /// <param name="subToNotes">OnNext action for the lists of notes.</param>
        /// <param name="tempo">Desired tempo of the recording. (notes will be emitted at the quarter note time based on this bpm)</param>
        /// <param name="scheduler">The scheduler on which to observe the notes.</param>
        /// <returns></returns>
        public IDisposable StartRecording(Action<(IList<Note>, int)> subToNotes, BPM tempo, IScheduler scheduler)
        {
            Tempo = tempo;
            StopWatch.Start();
            return GetNotesObservable()
                .ObserveOn(scheduler)
                .Select((notes, i) => (notes, i % 4))
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
