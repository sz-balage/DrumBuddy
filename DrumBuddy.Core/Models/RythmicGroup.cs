using System.Collections.Immutable;

namespace DrumBuddy.Core.Models
{
    public record RythmicGroup(ImmutableArray<Note> Notes);
}
