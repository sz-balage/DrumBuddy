using DrumBuddy.Core.Enums;
using DrumBuddy.IO.Services;

namespace DrumBuddy.IO.Abstractions;

public interface IMidiService
{
    public bool IsConnected { get; }
    public IObservable<bool> InputDeviceDisconnected { get; }
    public IObservable<int> GetRawNoteObservable();
    public MidiDeviceConnectionResult TryConnect();
}