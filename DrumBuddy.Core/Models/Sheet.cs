using DrumBuddy.IO.Extensions;
using DrumBuddy.IO.Models;

namespace DrumBuddy.Core.Models;

public class Sheet(Bpm tempo, List<Measure> measures, string name)
{
    public TimeSpan Length => CalculateLength();
    public string Name { get; init; } = name;

    public Bpm Tempo { get; } = tempo;

    public List<Measure> Measures { get; } = measures;

    private TimeSpan CalculateLength()
    {
        if (Measures.Count > 0)
            return (Measures.Count - 1) * (4 * Tempo.QuarterNoteDuration()) +
                   Measures.Last().Groups.Count(g => g.NoteGroups != null) * Tempo.QuarterNoteDuration();
        return TimeSpan.Zero;
    }
}