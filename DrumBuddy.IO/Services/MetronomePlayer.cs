using ManagedBass;

namespace DrumBuddy.IO.Services;

public class MetronomePlayer : IDisposable
{
    private readonly int _highBeep;
    private readonly int _normalBeep;

    public MetronomePlayer(string highBeepPath, string normalBeepPath)
    {
        if (!Bass.Init(-1, 44100, DeviceInitFlags.Default))
        {
            throw new Exception($"Failed to initialize Bass. Error: {Bass.LastError}");
        }

        _highBeep = Bass.CreateStream(highBeepPath, 0, 0, BassFlags.Default);
        if (_highBeep == 0)
            throw new Exception($"Failed to load high beep file. Error: {Bass.LastError}");

        _normalBeep = Bass.CreateStream(normalBeepPath, 0, 0, BassFlags.Default);
        if (_normalBeep == 0)
            throw new Exception($"Failed to load normal beep file. Error: {Bass.LastError}");
    }

    public void PlayHighBeep() => Bass.ChannelPlay(_highBeep,true);

    public void PlayNormalBeep() => Bass.ChannelPlay(_normalBeep, true);

    public void Dispose()
    {
        Bass.StreamFree(_highBeep);
        Bass.StreamFree(_normalBeep);
        Bass.Free();
    }
}