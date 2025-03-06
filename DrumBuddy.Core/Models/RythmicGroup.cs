using System.Collections.Immutable;

namespace DrumBuddy.Core.Models
{
    public record RythmicGroup(ImmutableArray<NoteGroup> NoteGroups)
    {
        public bool IsEmpty => NoteGroups.All(n => n.IsRest);
    }
}
