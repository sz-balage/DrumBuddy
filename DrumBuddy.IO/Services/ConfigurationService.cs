using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using DrumBuddy.Core.Abstractions;
using DrumBuddy.Core.Enums;
using DrumBuddy.IO.Services;

namespace DrumBuddy.IO;

public class ConfigurationService
{
    private readonly FileConfigurationStorage _storage;
    private readonly Dictionary<Drum, int> _mapping = new();
    private readonly BehaviorSubject<Drum?> _listeningDrum = new(null);

    public ConfigurationService(FileConfigurationStorage storage)
    {
        
        _storage = storage;
        _mapping = _storage.LoadConfig();
        if (_mapping.Count != 0) return;
        foreach (var drum in Enum.GetValues<Drum>())
            if (drum != Drum.Rest)
                _mapping[drum] = (int)drum;
    }

    public bool IsKeyboardEnabled { get; set; }
    public IReadOnlyDictionary<Drum, int> Mapping => _mapping;

    public Drum? ListeningDrum
    {
        get => _listeningDrum.Value;
        private set => _listeningDrum.OnNext(value);
    }

    public IObservable<Drum?> ListeningDrumChanged => _listeningDrum.AsObservable();

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
        var alreadyMappedDrum = _mapping.FirstOrDefault(kvp => kvp.Value == receivedNote).Key;
        if (alreadyMappedDrum != default) _mapping[alreadyMappedDrum] = -1;
        _mapping[ListeningDrum.Value] = receivedNote;
        _storage.SaveConfig(_mapping);
        StopListening();
    }
}