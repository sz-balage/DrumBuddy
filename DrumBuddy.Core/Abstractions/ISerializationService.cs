using System.Collections.Immutable;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Core.Abstractions;

public interface ISerializationService
{
    string SerializeMeasurementData(ImmutableArray<Measure> measures);
    ImmutableArray<Measure> DeserializeMeasurementData(string bytes);
}
