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
using LanguageExt;

[assembly: InternalsVisibleTo("DrumBuddy.Core.Unit")]
namespace DrumBuddy.Core.Services
{
    public static class RecordingService
    {
        /// <summary>
        /// Returns a sequence of metronome beeps, and starts the stopwatch.
        /// </summary>
        /// <returns>The index of the current beep from 0-3, resetting on each measure.</returns>
        public static IObservable<long> GetMetronomeBeeping(BPM bpm) => Observable.Interval(bpm.QuarterNoteDuration())
                                                                    .Select(i => i%4)
                                                                    .Publish()
                                                                    .AutoConnect(2);
        public static IObservable<IList<Note>> GetNotes(BPM bpm, IObservable<Beat> Beats) =>
                        Beats.Select(b => new Note(b,NoteValue.Sixteenth))
                        .Buffer(bpm.SixteenthNoteDuration())
                        .Select(notes => notes.Count == 0 ? Prelude.List(new Note(Beat.Rest, NoteValue.Sixteenth)).ToList() : notes);
    }
}
