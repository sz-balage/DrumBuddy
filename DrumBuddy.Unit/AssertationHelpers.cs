using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Enums;
using Shouldly;

namespace DrumBuddy.Core.Unit;

public static class AssertationHelpers
{
    public static void ShouldHaveBeatAndValue(this Note note, Drum drum, NoteValue value)
    {
        note.Drum.ShouldBe(drum);
        note.Value.ShouldBe(value);
    }
}