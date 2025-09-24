using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;

namespace DrumBuddy.DesignHelpers;

public static class TestSheetProvider
{
    public static Sheet GetTestSheet()
    {
        return new Sheet(100,
            [new Measure([new RythmicGroup([new NoteGroup([new Note(Drum.Crash, NoteValue.Eighth)])])])],
            "Test",
            "Test");
    }
}