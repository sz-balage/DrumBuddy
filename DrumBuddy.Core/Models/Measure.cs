using System.Text.Json.Serialization;

namespace DrumBuddy.Core.Models;

/// <summary>
///     Represents a single measure of music.
/// </summary>
/// <param name="Groups">The 4 rythmic groups that make up the measure.</param>
public record Measure(List<RythmicGroup> Groups)
{
    [JsonIgnore]
    public bool IsEmpty => Groups.Count == 0;
}