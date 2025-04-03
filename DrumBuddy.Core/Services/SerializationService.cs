using System.Collections.Immutable;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using DrumBuddy.Core.Abstractions;
using DrumBuddy.Core.Helpers;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Core.Services;


public class SerializationService : ISerializationService
{
    public byte[] SerializeMeasurementData(ImmutableArray<Measure> measures)
    {
        return JsonSerializer.SerializeToUtf8Bytes(measures);
    }
    //TODO: not very good solution, look at why value isnt deserialized, OR make it computed value
    public ImmutableArray<Measure> DeserializeMeasurementData(byte[] json)
    {
        var measures = JsonSerializer.Deserialize<IEnumerable<Measure>>(json)
            ?? throw new InvalidOperationException("Failed to deserialize sheet");
        foreach (var measure in measures)
        {
            foreach (var measureGroup in measure.Groups)
            {
                foreach (var noteGroup in measureGroup.NoteGroups)
                {
                    noteGroup.RefreshValue();
                }
            }
        }
        return [..measures];
    }
}