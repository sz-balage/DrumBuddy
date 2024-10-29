using System.Reactive.Concurrency;
using DrumBuddy.IO.Enums;
using DrumBuddy.IO.Extensions;
using DrumBuddy.IO.Models;
using System.Reactive.Linq;
using DrumBuddy.IO.Abstractions;

namespace DrumBuddy.IO.Services
{
    public class MidiService : IMidiService
    {
        public IObservable<Beat> GetBeatsObservable(BPM tempo) //for now for testing purposes it only returns a beat every sixteenth note duration (calculated from BPM
         => Observable.Interval(tempo.QuarterNoteDuration())
                       .Select(_ => Beat.Snare);
    }
}
