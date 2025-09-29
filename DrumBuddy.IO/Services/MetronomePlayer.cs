using ManagedBass;

namespace DrumBuddy.IO.Services;

public class MetronomePlayer : IDisposable
{
    private readonly int _highBeep;
    private readonly int _normalBeep;

    public MetronomePlayer(string highBeepPath, string normalBeepPath)
    {
        if (!Bass.Init()) throw new Exception($"Failed to initialize Bass. Error: {Bass.LastError}");

        _highBeep = Bass.CreateStream(highBeepPath);
        if (_highBeep == 0)
            throw new Exception($"Failed to load high beep file. Error: {Bass.LastError}");

        _normalBeep = Bass.CreateStream(normalBeepPath);
        if (_normalBeep == 0)
            throw new Exception($"Failed to load normal beep file. Error: {Bass.LastError}");
    }

    public void Dispose()
    {
        Bass.StreamFree(_highBeep);
        Bass.StreamFree(_normalBeep);
        Bass.Free();
    }

    /// <summary>
    ///     Sets volume of bass streams globally.
    /// </summary>
    /// <param name="volume">Level of volume, from 0 (silent) to 10000 (max)</param>
    public void SetVolume(int volume)
    {
        Bass.GlobalStreamVolume = volume;
    }

    public void PlayHighBeep()
    {
        Bass.ChannelPlay(_highBeep, true);
    }

    public void PlayNormalBeep()
    {
        Bass.ChannelPlay(_normalBeep, true);
    }
}