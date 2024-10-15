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
        /// <summary>
        /// ONLY FOR TESTING PURPOSES, THE MIDI SERVICE WILL NOT HAVE A TEMPO PROPERTY
        /// </summary>
        public BPM Tempo = (BPM)BPM.From(100); //default value is 100
        public IObservable<Beat> GetBeatsObservable() //for now for testing purposes it only returns a beat every sixteenth note duration (calculated from BPM
         => Observable.Interval(Tempo.QuarterNoteDuration())
                       .Select(_ => new Beat(DrumType.Snare));
    }
}
