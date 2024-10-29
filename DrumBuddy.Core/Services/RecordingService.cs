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
        public RecordingService(IMidiService midiService = null, IScheduler scheduler = null)
        {
            _scheduler = scheduler ?? Scheduler.Default;
            _midiService = midiService ?? new MidiService();
        }
        /// <summary>
        /// Returns a sequence of metronome beeps, and starts the stopwatch.
        /// </summary>
        /// <returns>The index of the current beep from 0-3, resetting on each measure.</returns>
        public IObservable<long> GetMetronomeBeeping(BPM bpm) => Observable.Interval(bpm.QuarterNoteDuration(), _scheduler)
                                                                    .Select(i => i%4)
                                                                    .Publish()
                                                                    .AutoConnect(2);
        public IObservable<IList<Beat>> GetBeats(BPM bpm) =>
            _midiService.GetBeatsObservable(bpm)
                        .Buffer(bpm.SixteenthNoteDuration());
        public Stopwatch StopWatch { get; } = new();
    }
}
