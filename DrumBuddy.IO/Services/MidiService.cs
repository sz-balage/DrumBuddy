using System.Reactive.Concurrency;
using DrumBuddy.IO.Enums;
using DrumBuddy.IO.Extensions;
using DrumBuddy.IO.Models;
using System.Reactive.Linq;
using DrumBuddy.IO.Abstractions;
using LanguageExt;
using LanguageExt.Common;
using Melanchall.DryWetMidi.Core;
using static LanguageExt.Prelude;
using Melanchall.DryWetMidi.Multimedia;

namespace DrumBuddy.IO.Services
{
    public class MidiService : IMidiService
    {
        private IInputDevice _device;
        static readonly Error NoDeviceFound = Error.New(200, "There are no MIDI devices connected.");
        ///<summary>
        /// Gets the connected MIDI devices.
        /// </summary>
        /// <returns>An error if no devices are connected, or the list of devices</returns>
        public Fin<Seq<DeviceName>> GetAvailableDevices()
            => InputDevice.GetAll()
                .Match(
                    Empty: () => FinFail<Seq<DeviceName>>(NoDeviceFound),
                    More: devices => FinSucc(devices.Map(id => new DeviceName(id.Name)).ToSeq()));
        public Fin<Unit> ConnectDevice(DeviceName deviceName) => Eff(() =>
        {
            _device = InputDevice.GetByName(deviceName.Value);
            return unit;
        }).Run();
        private IObservable<NoteOnEvent> _midiInput = Observable.Empty<NoteOnEvent>();
        public bool IsConnected { get; private set; } = false;
        public IObservable<Beat> GetBeatsObservable() =>
            _midiInput.Select(args => (Beat)int.Parse(args.NoteNumber.ToString()))
                .Buffer(2)
                .Select(list => list.First());
    }
}