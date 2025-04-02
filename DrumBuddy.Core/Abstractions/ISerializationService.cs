using System.Collections.Immutable;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Core.Abstractions;

public interface ISerializationService
{
    byte[] SerializeMeasurementData(ImmutableArray<Measure> measures);
    ImmutableArray<Measure> DeserializeMeasurementData(byte[] bytes);
}
