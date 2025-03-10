using DrumBuddy.IO.Enums;
using DrumBuddy.IO.Services;

namespace DrumBuddy.IO.Abstractions
{
    public interface IMidiService
    {
        public IObservable<Beat> GetBeatsObservable();
        public bool IsConnected { get; }
        public IObservable<bool> InputDeviceDisconnected { get; }
        public MidiDeviceConnectionResult TryConnect();

    }
}
