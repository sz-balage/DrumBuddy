using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using Shouldly;

namespace DrumBuddy.Core.Unit;

public static class AssertationHelpers
{
    public static void ShouldHaveBeatAndValue(this Note note, Drum expectedDrum, NoteValue expectedValue)
    {
        note.ShouldNotBeNull();
        note.Drum.ShouldBe(expectedDrum, $"Expected drum {expectedDrum}, but got {note.Drum}");
        note.Value.ShouldBe(expectedValue, $"Expected value {expectedValue}, but got {note.Value}");
    }

    public static void ShouldHaveDrum(this Note note, Drum expectedDrum)
    {
        note.ShouldNotBeNull();
        note.Drum.ShouldBe(expectedDrum);
    }

    public static void ShouldHaveValue(this Note note, NoteValue expectedValue)
    {
        note.ShouldNotBeNull();
        note.Value.ShouldBe(expectedValue);
    }

    public static void ShouldBeAKickDrum(this Note note)
    {
        note.Drum.ShouldBe(Drum.Kick);
    }

    public static void ShouldBeASnare(this Note note)
    {
        note.Drum.ShouldBe(Drum.Snare);
    }

    public static void ShouldBeARest(this Note note)
    {
        note.Drum.ShouldBe(Drum.Rest);
    }

    public static void ShouldHaveExactlyNoteCount(this NoteGroup noteGroup, int expectedCount)
    {
        noteGroup.ShouldNotBeNull();
        noteGroup.Count.ShouldBe(expectedCount, $"Expected {expectedCount} notes, but got {noteGroup.Count}");
    }

    public static void ShouldHaveExactlyGroupCount(this List<NoteGroup> noteGroups, int expectedCount)
    {
        noteGroups.ShouldNotBeNull();
        noteGroups.Count.ShouldBe(expectedCount, $"Expected {expectedCount} note groups, but got {noteGroups.Count}");
    }
}