using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using ManagedBass.Midi;

namespace DrumBuddy.IO.Services;

public class MidiService
{
    private readonly Subject<bool> _inputDeviceDisconnected = new();
    private readonly Subject<int> _notes = new();
    private bool _isConnected;

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

    public IObservable<int> GetRawNoteObservable()
    {
        return IsConnected
            ? _notes
            : Observable.Empty<int>();
    }


    public MidiDeviceConnectionResult TryConnect()
    {
        var devCount = BassMidi.InDeviceCount;

        if (devCount == 0)
            return new MidiDeviceConnectionResult([]);
        if (devCount > 1)
        {
            var devices = new MidiDeviceShortInfo[devCount];
            for (var i = 0; i < devCount; i++)
            {
                BassMidi.InFree(i);
                BassMidi.InGetDeviceInfo(i, out var info);
                devices[i] = new MidiDeviceShortInfo(info.ID, info.Name);
            }

            return new MidiDeviceConnectionResult(devices);
        }

        SetDeviceForIdx(0);
        var singleDeviceInfo = BassMidi.InGetDeviceInfo(0);
        return new MidiDeviceConnectionResult([new MidiDeviceShortInfo(singleDeviceInfo.ID, singleDeviceInfo.Name)]);
    }

    private void MidiInCallback(int device, double time, IntPtr buffer, int length, IntPtr user)
    {
        if (length == 0) return;
        var midiData = new byte[length];
        Marshal.Copy(buffer, midiData, 0, length);
        if (midiData.Length >= 3)
        {
            var status = midiData[0];
            var data1 = midiData[1];
            var data2 = midiData[2];

            if ((status & 0xF0) == 0x90 && data2 > 0) _notes.OnNext(data1);
        }
    }

    public void SetUserChosenDeviceAsInput(MidiDeviceShortInfo? chosenDeviceInfo)
    {
        var indexOfChosenDevice = 0;
        for (var i = 0; i < BassMidi.InDeviceCount; i++)
            if (BassMidi.InGetDeviceInfo(i).Name == chosenDeviceInfo?.Name)
            {
                indexOfChosenDevice = i;
                break;
            }

        SetDeviceForIdx(indexOfChosenDevice);
    }

    private void SetDeviceForIdx(int idx)
    {
        BassMidi.InFree(idx); //to avoid double init
        if (BassMidi.InInit(idx, MidiInCallback, IntPtr.Zero))
            if (BassMidi.InStart(idx))
                IsConnected = true;
    }
}

public record MidiDeviceConnectionResult(MidiDeviceShortInfo[] DevicesConnected);

public record MidiDeviceShortInfo(int Id, string Name);