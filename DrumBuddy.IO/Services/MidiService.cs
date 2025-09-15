using System.Reactive.Linq;
using System.Reactive.Subjects;
using DrumBuddy.Core.Enums;
using DrumBuddy.IO.Abstractions;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace DrumBuddy.IO.Services;

public class MidiService : IMidiService
{
    private readonly DevicesWatcher _devicesWatcher;
    private readonly Subject<bool> _inputDeviceDisconnected = new();

    private readonly IObservable<NoteOnEvent> _midiInput = Observable.Empty<NoteOnEvent>();
    private InputDevice _device;
    private bool _isConnected;

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
            return new MidiDeviceConnectionResult(false,
                "Multiple MIDI devices connected. Please remove any additional devices.");
        SetDevice(devices.Single());
        return new MidiDeviceConnectionResult(true, null);
    }

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            _isConnected = value;
            if (value == false)
                _inputDeviceDisconnected.OnNext(_isConnected);
        }
    }

    public IObservable<bool> InputDeviceDisconnected => _inputDeviceDisconnected;

    public IObservable<int> GetRawNoteObservable()
    {
        return IsConnected
            ? _midiInput.Select(evt => (int)evt.NoteNumber)
                .Buffer(2)
                .Select(list => list.First())
            : Observable.Empty<int>();
    }

    private void OnDeviceRemoved(object? sender, DeviceAddedRemovedEventArgs e)
    {
        if (e.Device is InputDevice device && device.Name == _device.Name)
            IsConnected = false;
    }

    private void SetDevice(InputDevice newDevice)
    {
        _device = newDevice;
        IsConnected = true;
    }
}

public record MidiDeviceConnectionResult(bool IsSuccess, string? Message);