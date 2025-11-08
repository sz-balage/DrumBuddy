using System.Collections.Immutable;
using DrumBuddy.Core.Extensions;

namespace DrumBuddy.Core.Models;

public class Sheet
{
    public Sheet(Bpm tempo, ImmutableArray<Measure> measures, string name, string description, Guid id = default)
    {
        Name = name;
        Description = description;
        Tempo = tempo;
        Measures = measures;
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
    }
    public Guid Id { get; init; }
    public DateTime? LastSyncedAt { get; set; }
    public string Name { get; init; }
    public string Description { get; init; }

    public Bpm Tempo { get; set; }
    public ImmutableArray<Measure> Measures { get; }

    //client specific
    public bool IsSyncEnabled { get; set; }
    public TimeSpan Length => CalculateLength();
 
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