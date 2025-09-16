using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using DrumBuddy.Core.Enums;

namespace DrumBuddy.Core.Services;

public class ConfigurationService
{
    private readonly Dictionary<Drum, int> _mapping = new();
    private readonly BehaviorSubject<Drum?> _listeningDrum = new(null);

    public ConfigurationService()
    {
        foreach (var drum in Enum.GetValues<Drum>())
            if (drum != Drum.Rest)
                _mapping[drum] = (int)drum;
    }

    public IReadOnlyDictionary<Drum, int> Mapping => _mapping;

    public Drum? ListeningDrum
    {
        get => _listeningDrum.Value;
        private set => _listeningDrum.OnNext(value);
    }

    public IObservable<Drum?> ListeningDrumChanged => _listeningDrum.AsObservable();

    public void StartListening(Drum drum) => ListeningDrum = drum;

    public void StopListening() => ListeningDrum = null;

    public void MapDrum(int receivedNote)
    {
        if (ListeningDrum is null || receivedNote < 0)
            return;
        var alreadyMappedDrum = _mapping.FirstOrDefault(kvp => kvp.Value == receivedNote).Key;
        if (alreadyMappedDrum != default)
        {
            _mapping[alreadyMappedDrum] = -1; 
        }
        _mapping[ListeningDrum.Value] = receivedNote;
        StopListening();
    }

    public void SaveConfig(string path) => //TODO: call this
        File.WriteAllText(path, JsonSerializer.Serialize(_mapping));

    public void LoadConfig(string path)
    {
        if (!File.Exists(path)) return;
        var data = JsonSerializer.Deserialize<Dictionary<Drum, int>>(File.ReadAllText(path));
        if (data is not null)
            foreach (var kvp in data)
                _mapping[kvp.Key] = kvp.Value;
    }
}