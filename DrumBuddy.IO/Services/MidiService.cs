using System.Reactive.Linq;
using System.Reactive.Subjects;
using DrumBuddy.IO.Abstractions;
using RtMidi.Core;
using RtMidi.Core.Devices;
using RtMidi.Core.Messages;

namespace DrumBuddy.IO.Services;

public class MidiService : IMidiService
{
    private readonly Subject<bool> _inputDeviceDisconnected = new();
    private readonly Subject<int> _notes = new();
    private IMidiInputDevice _device;
    private bool _isConnected;

    public IObservable<int> GetRawNoteObservable()
    {
        return IsConnected
            ? _notes
            : Observable.Empty<int>();
    }

    public MidiDeviceConnectionResult TryConnect()
    {
        var devices = MidiDeviceManager.Default.InputDevices.ToList();
        if (!devices.Any())
            return new MidiDeviceConnectionResult(false, "No MIDI devices connected.");
        if (devices.Count() > 1)
            return new MidiDeviceConnectionResult(false,
                "Multiple MIDI devices connected. Please remove any additional devices.");

        _device = devices.First().CreateDevice();
        _device.NoteOn += OnNoteOn;
        _device.Open();
        return new MidiDeviceConnectionResult(true, $"{_device.Name} connected successfully.");
    }

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            _isConnected = value;
            if (!value)
                _inputDeviceDisconnected.OnNext(_isConnected);
        }
    }

    public IObservable<bool> InputDeviceDisconnected => _inputDeviceDisconnected;

    private void OnNoteOn(IMidiInputDevice sender, in NoteOnMessage msg)
    {
        _notes.OnNext((int)msg.Key);
    }
}

public record MidiDeviceConnectionResult(bool IsSuccess, string? Message);