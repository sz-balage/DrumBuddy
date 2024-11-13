using DrumBuddy.IO.Enums;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DrumBuddy.IO.Abstractions;
using LanguageExt;
using LanguageExt.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using static LanguageExt.Prelude;

namespace DrumBuddy.IO.Services
{
    public class MidiService : IMidiService
    {
        private InputDevice _device;
        private readonly DevicesWatcher _devicesWatcher;

        private IObservable<NoteOnEvent> _midiInput = Observable.Empty<NoteOnEvent>();
        public MidiService()
        {
            _devicesWatcher = DevicesWatcher.Instance;
            _devicesWatcher.DeviceRemoved += OnDeviceRemoved;
        }
        public Fin<Unit> TryConnect() =>
            InputDevice.GetAll().Match(
            Empty: () => FinFail<Unit>(Error.New("Couldn't find any MIDI devices connected to the computer.")),
            More: devices => devices.Count == 1
                ? SetDevice(devices.Single())
                : FinFail<Unit>(Error.New("More than one MIDI device connected. Please remove any additional devices.")));
        private void OnDeviceRemoved(object? sender, DeviceAddedRemovedEventArgs e)
        {
            if (e.Device is InputDevice device && device.Name == _device.Name)
                IsConnected = false;
        }
        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                _isConnected = value;
                if(value == false)
                    _inputDeviceDisconnected.OnNext(_isConnected);
            }
        }
        private readonly Subject<bool> _inputDeviceDisconnected = new();
        public IObservable<bool> InputDeviceDisconnected => _inputDeviceDisconnected;
        private Unit SetDevice(InputDevice newDevice)
        {
            _device = newDevice;
            IsConnected = true;
            return unit;
        }
        public IObservable<Beat> GetBeatsObservable() => IsConnected ? 
            _midiInput.Select(args => (Beat)int.Parse(args.NoteNumber.ToString()))
                .Buffer(2)
                .Select(list => list.First()) 
            : Observable.Empty<Beat>();
    }
}