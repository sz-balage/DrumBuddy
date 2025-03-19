using System.Collections.Immutable;
using DrumBuddy.Core.Extensions;

namespace DrumBuddy.Core.Models;

public class Sheet(Bpm tempo, ImmutableArray<Measure> measures, string name)
{
    public TimeSpan Length => CalculateLength();
    public string Name { get; init; } = name;

    public Bpm Tempo { get; } = tempo;

    public ImmutableArray<Measure> Measures { get; } = measures;

    private TimeSpan CalculateLength()
    {
        if (Measures.Length > 0)
            return (Measures.Length - 1) * (4 * Tempo.QuarterNoteDuration()) +
                   Measures.Last().Groups.Count(g => g.NoteGroups != null) * Tempo.QuarterNoteDuration();
        return TimeSpan.Zero;
    }
}