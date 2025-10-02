using System.Collections.Immutable;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Core.Abstractions;

public interface ISerializationService
{
    byte[] SerializeMeasurementData(ImmutableArray<Measure> measures);
    ImmutableArray<Measure> DeserializeMeasurementData(byte[] bytes);
    string SerializeAppConfiguration(AppConfiguration appConfig);
    AppConfiguration? DeserializeAppConfiguration(string appConfigJson);
}