using System.Collections.Immutable;

namespace DrumBuddy.Core.Models
{
    /// <summary>
    /// Represents a group of notes that are played in a time window of one quarter of a measure.
    /// </summary>
    /// <param name="NoteGroups">Note groups inside the rythmic group</param>
    public record RythmicGroup(ImmutableArray<NoteGroup> NoteGroups)
    {
        public bool IsEmpty => NoteGroups.All(n => n.IsRest);
    }
}
