using System.Collections.Immutable;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Core.Abstractions;

public interface ISerializationService
{
    byte[] SerializeMeasurementData(ImmutableArray<Measure> measures);
    ImmutableArray<Measure> DeserializeMeasurementData(byte[] bytes);
    string SerializeDrumMappingData(Dictionary<Drum, int> mapping);
    Dictionary<Drum, int>? DeserializeDrumMappingData(string mappingJson);
}
