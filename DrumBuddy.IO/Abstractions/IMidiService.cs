using DrumBuddy.IO.Enums;
using DrumBuddy.IO.Services;

namespace DrumBuddy.IO.Abstractions;

public interface IMidiService
{
    public bool IsConnected { get; }
    public IObservable<bool> InputDeviceDisconnected { get; }
    public IObservable<Drum> GetBeatsObservable();
    public MidiDeviceConnectionResult TryConnect();
}