using System.Collections.Immutable;
using System.Text.Json;
using DrumBuddy.Core.Abstractions;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Core.Services;

public class SerializationService : ISerializationService
{
    public byte[] SerializeMeasurementData(ImmutableArray<Measure> measures)
    {
        return JsonSerializer.SerializeToUtf8Bytes(measures);
    }

    public ImmutableArray<Measure> DeserializeMeasurementData(byte[] json)
    {
        var measures = JsonSerializer.Deserialize<IEnumerable<Measure>>(json)
                       ?? throw new InvalidOperationException("Failed to deserialize sheet");
        return [..measures];
    }

    public string SerializeDrumMappingData(Dictionary<Drum, int> mapping)
    {
        return JsonSerializer.Serialize(mapping);
    }

    public Dictionary<Drum, int>? DeserializeDrumMappingData(string mappingJson)
    {
        return JsonSerializer.Deserialize<Dictionary<Drum, int>>(mappingJson);
    }
}