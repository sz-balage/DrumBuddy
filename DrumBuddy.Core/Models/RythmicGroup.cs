using System.Collections.Immutable;
using DrumBuddy.IO.Enums;

namespace DrumBuddy.Core.Models
{
    public record RythmicGroup(ImmutableArray<Note> Notes)
    {
        public bool IsEmpty => Notes.All(n => n.Beat == Beat.Rest);
    }
}
