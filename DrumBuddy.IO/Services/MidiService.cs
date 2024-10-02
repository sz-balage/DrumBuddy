using DrumBuddy.IO.Enums;
using DrumBuddy.IO.Extensions;
using DrumBuddy.IO.Models;
using System.Reactive.Linq;

namespace DrumBuddy.IO.Services
{
    public class MidiService
    {
        private BPM _tempo;

        public MidiService(BPM tempo)
        {
            _tempo = tempo;
        }
        public IObservable<Beat> GetBeatsObservable() //for now for testing purposes it only returns a beat every sixteenth note duration (calculated from BPM
         => Observable.Interval(_tempo.QuarterNoteDuration())
                       .Select(_ => new Beat(DateTime.Now, DrumType.Snare));
    }
}
