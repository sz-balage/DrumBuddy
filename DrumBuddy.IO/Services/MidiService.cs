using DrumBuddy.IO.Enums;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DrumBuddy.IO.Abstractions;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

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
        public MidiDeviceConnectionResult TryConnect()
        {
            var devices = InputDevice.GetAll();
            if (devices.Count == 0)
                return new MidiDeviceConnectionResult(false, "No MIDI devices connected.");
            if (devices.Count > 1)
                return new MidiDeviceConnectionResult(false, "Multiple MIDI devices connected. Please remove any additional devices.");
            SetDevice(devices.Single());
            return new MidiDeviceConnectionResult(true, null);
            // return InputDevice.GetAll().Match(
            //     Empty: () => new MidiDeviceConnectionError("No MIDI devices connected."),
            //     More: devices => devices.Count == 1
            //         ? Either<MidiDeviceConnectionError, Unit>.Right(SetDevice(devices.Single()))
            //         : new MidiDeviceConnectionError(
            //             "More than one MIDI device connected. Please remove any additional devices."));
        }

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
        private void SetDevice(InputDevice newDevice)
        {
            _device = newDevice;
            IsConnected = true;
        }
        public IObservable<Drum> GetBeatsObservable() => IsConnected ? 
            _midiInput.Select(args => (Drum)int.Parse(args.NoteNumber.ToString()))
                .Buffer(2)
                .Select(list => list.First()) 
            : Observable.Empty<Drum>();
    }

    public record MidiDeviceConnectionResult(bool IsSuccess,string? Message);
}