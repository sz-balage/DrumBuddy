using System.Reactive.Linq;
using System.Reactive.Subjects;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Data.Storage;

namespace DrumBuddy.IO.Services;

public class ConfigurationService
{
    private readonly BehaviorSubject<Drum?> _listeningDrum = new(null);
    private readonly MetronomePlayer _metronomePlayer;
    private readonly FileConfigurationStorage _storage;
    private AppConfiguration _config;
    public ConfigurationService(FileConfigurationStorage storage,
        MetronomePlayer metronomePlayer)
    {
        _storage = storage;
        _metronomePlayer = metronomePlayer;
        _config = _storage.LoadConfig();
        if (_config.DrumMapping.Count == 0)
        {
            foreach (var drum in Enum.GetValues<Drum>())
                if (drum != Drum.Rest)
                    _config.DrumMapping[drum] = (int)drum;
        }
        if (_config.DrumPositions.Count == 0)
        {
            foreach (var drum in Enum.GetValues<Drum>())
                if (drum != Drum.Rest)
                    _config.DrumPositions[drum] = DefaultYPosition(drum);
        }
        
    }

    public IReadOnlyDictionary<Drum, double> DrumPositions => _config.DrumPositions;
    public int MetronomeVolume
    {
        get => _config.MetronomeVolume;
        set { _config.MetronomeVolume = value; Save(); _metronomePlayer?.SetVolume(value);}
    }

    private void Save() => _storage.SaveConfig(_config);
    public bool KeyboardInput 
    {
        get => _config.KeyboardInput;
        set { _config.KeyboardInput = value; Save(); }
    }
    public IReadOnlyDictionary<Drum, int> Mapping => _config.DrumMapping;
    public Drum? ListeningDrum
    {
        get => _listeningDrum.Value;
        private set => _listeningDrum.OnNext(value);
    }

    public IObservable<Drum?> ListeningDrumChanged => _listeningDrum.AsObservable();

    private static double DefaultYPosition(Drum drum)
    {
        return drum switch
        {
            Drum.Kick => 60,
            Drum.Snare => 20,
            Drum.FloorTom => 40,
            Drum.Tom1 => 0,
            Drum.Tom2 => 10,
            Drum.Ride => -10,
            Drum.HiHat => -20,
            Drum.HiHat_Pedal => 85,
            Drum.Crash1 => -30,
            Drum.Crash2 => -30,
            _ => 30
        };
    }

    public void StartListening(Drum drum)
    {
        ListeningDrum = drum;
    }

    public void StopListening()
    {
        ListeningDrum = null;
    }

    public void MapDrum(int receivedNote)
    {
        if (ListeningDrum is null || receivedNote < 0)
            return;
        var alreadyMappedDrum = Mapping.FirstOrDefault(kvp => kvp.Value == receivedNote).Key;
        if (alreadyMappedDrum != default) _config.DrumMapping[alreadyMappedDrum] = -1;
        _config.DrumMapping[ListeningDrum.Value] = receivedNote;
        Save();
        StopListening();
    }
}