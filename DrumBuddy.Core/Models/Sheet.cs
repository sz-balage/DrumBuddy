using System.Collections.Immutable;
using System.Text.Json.Serialization;
using DrumBuddy.Core.Extensions;

namespace DrumBuddy.Core.Models;

public class Sheet(Bpm tempo, ImmutableArray<Measure> measures, string name, string description)
{
    public TimeSpan Length => CalculateLength();
    public string Name { get; init; } = name;
    public string Description { get; init; } = description;

    public Bpm Tempo { get; } = tempo;
    public ImmutableArray<Measure> Measures { get; } = measures;

    private TimeSpan CalculateLength()
    {
        if (Measures.Length > 0)
            return (Measures.Length - 1) * (4 * Tempo.QuarterNoteDuration()) +
                   Measures.Last().Groups.Count(g => g.NoteGroups != null) * Tempo.QuarterNoteDuration();
        return TimeSpan.Zero;
    }

    public Sheet RenameSheet(string newName, string newDescription)
    {
        return new Sheet(Tempo, Measures, newName, newDescription);
    }
}