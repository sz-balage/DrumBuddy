using DrumBuddy.IO.Models;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace DrumBuddy.IO.Services;

public class MetronomePlayer : IDisposable
{
    private readonly CachedSound _highBeep;
    private readonly CachedSound _normalBeep;
    private readonly WaveOutEvent _outputDevice;
    private readonly MixingSampleProvider _mixer;

    public MetronomePlayer(string highBeepPath, string normalBeepPath)
    {
        _highBeep = new CachedSound(highBeepPath);
        _normalBeep = new CachedSound(normalBeepPath);

        _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2))
        {
            ReadFully = true
        };

        _outputDevice = new WaveOutEvent();
        _outputDevice.Init(_mixer);
        _outputDevice.Play();
    }

    public void PlayHighBeep()
    {
        _mixer.AddMixerInput(new CachedSoundSampleProvider(_highBeep));
    }

    public void PlayNormalBeep()
    {
        _mixer.AddMixerInput(new CachedSoundSampleProvider(_normalBeep));
    }

    public void Dispose()
    {
        _outputDevice.Dispose();
    }
}