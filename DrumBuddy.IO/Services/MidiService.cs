using System.Reactive.Concurrency;
using DrumBuddy.IO.Enums;
using DrumBuddy.IO.Extensions;
using DrumBuddy.IO.Models;
using System.Reactive.Linq;
using DrumBuddy.IO.Abstractions;
using LanguageExt;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace DrumBuddy.IO.Services
{
    public class MidiService : IMidiService
    {
        private IInputDevice _device;

        public MidiService()
        { 
            _device = InputDevice.GetAll().Single(); //Can be: 0 - no midi dev connected,
                                            //1 - one dev -> connect that,
                                            //2 - multiple devices -> user has to choose 
            _device.StartEventsListening();
            _midiInput = Observable.FromEventPattern<MidiEventReceivedEventArgs>(_device, nameof(_device.EventReceived))
                .Where(ep => ep.EventArgs.Event is NoteOnEvent)
                .Select(ep => (NoteOnEvent)ep.EventArgs.Event);
            
        }

        private IObservable<NoteOnEvent> _midiInput;
        
        
        public IObservable<Beat>
            GetBeatsObservable(
                BPM tempo) //for now for testing purposes it only returns a beat every sixteenth note duration (calculated from BPM
            => _midiInput.Select(args => (Beat)int.Parse(args.NoteNumber.ToString()))
                .Buffer(2)
                .Select(list => list.First());
    }
}
