using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Enums;
using Shouldly;

namespace DrumBuddy.Core.Unit;

public class WhenNotesAreUpscaled
{
    [Theory]
    [InlineData(Beat.Snare)]
    [InlineData(Beat.Bass)]
    [InlineData(Beat.HiHat)]
    [InlineData(Beat.Crash1)]
    [InlineData(Beat.Crash2)]
    [InlineData(Beat.Ride)]
    [InlineData(Beat.Tom1)]
    [InlineData(Beat.Tom2)]
    [InlineData(Beat.FloorTom)]
    public void WithOneNoteFollowedByThreeRests_UpscalesToQuarter(Beat beat)
    {
        // Arrange
        var noteGroups = new List<NoteGroup>
        {
            new() { new Note(beat, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) }
        };

        // Act
        var result = RecordingService.UpscaleNotes(noteGroups);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Count.ShouldBe(1);
        result[0][0].ShouldHaveBeatAndValue(beat, NoteValue.Quarter);
    }

    [Theory]
    [InlineData(Beat.Snare, Beat.Snare)]
    [InlineData(Beat.Bass, Beat.Bass)]
    [InlineData(Beat.Snare, Beat.Bass)]
    public void WithFourConsecutiveNotes_KeepsThemDistinct(Beat beat1, Beat beat2)
    {
        // Arrange
        var noteGroups = new List<NoteGroup>
        {
            new() { new Note(beat1, NoteValue.Sixteenth) },
            new() { new Note(beat2, NoteValue.Sixteenth) },
            new() { new Note(beat1, NoteValue.Sixteenth) },
            new() { new Note(beat2, NoteValue.Sixteenth) }
        };

        // Act
        var result = RecordingService.UpscaleNotes(noteGroups);

        // Assert
        result.Count.ShouldBe(4);
        for (var i = 0; i < 4; i++)
        {
            result[i].Count.ShouldBe(1);
            if (i == 0 || i == 2)
                result[i][0].ShouldHaveBeatAndValue(beat1, NoteValue.Sixteenth);
            else
                result[i][0].ShouldHaveBeatAndValue(beat2, NoteValue.Sixteenth);
        }
    }

    [Fact]
    public void WithConsecutiveRests_UpscalesNotes()
    {
        // Arrange
        var noteGroups = new List<NoteGroup>
        {
            new() { new Note(Beat.Snare, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(Beat.Snare, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) }
        };

        // Act
        var result = RecordingService.UpscaleNotes(noteGroups);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Count.ShouldBe(1);
        result[1].Count.ShouldBe(1);
        result[0][0].ShouldHaveBeatAndValue(Beat.Snare, NoteValue.Eighth);
        result[1][0].ShouldHaveBeatAndValue(Beat.Snare, NoteValue.Eighth);
    }

    [Theory]
    [InlineData(Beat.Snare, Beat.Snare)]
    [InlineData(Beat.Bass, Beat.Bass)]
    [InlineData(Beat.Snare, Beat.Bass)]
    public void WithOneNoteFollowedByTwoRestsAndOneNote_UpscalesToNoteRestNote(Beat beat1, Beat beat2)
    {
        var noteGroups = new List<NoteGroup>
        {
            new() { new Note(beat1, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(beat2, NoteValue.Sixteenth) }
        };

        var result = RecordingService.UpscaleNotes(noteGroups);

        result.Count.ShouldBe(3);
        result[0].Count.ShouldBe(1);
        result[1].Count.ShouldBe(1);
        result[2].Count.ShouldBe(1);
        result[0][0].ShouldHaveBeatAndValue(beat1, NoteValue.Eighth);
        result[1][0].ShouldHaveBeatAndValue(Beat.Rest, NoteValue.Sixteenth);
        result[2][0].ShouldHaveBeatAndValue(beat2, NoteValue.Sixteenth);
    }

    [Fact]
    public void WithFourRests_UpscalesToOneQuarterRest()
    {
        // Arrange
        var noteGroups = new List<NoteGroup>
        {
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) }
        };

        // Act
        var result = RecordingService.UpscaleNotes(noteGroups);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Count.ShouldBe(1);
        result[0][0].ShouldHaveBeatAndValue(Beat.Rest, NoteValue.Quarter);
    }

    [Fact]
    public void WithTwoRestsFollowedByNoteAndRest_UpscalesToOneRestOneNote()
    {
        // Arrange
        var noteGroups = new List<NoteGroup>
        {
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(Beat.Snare, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) }
        };

        // Act
        var result = RecordingService.UpscaleNotes(noteGroups);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Count.ShouldBe(1);
        result[1].Count.ShouldBe(1);
        result[0][0].ShouldHaveBeatAndValue(Beat.Rest, NoteValue.Eighth);
        result[1][0].ShouldHaveBeatAndValue(Beat.Snare, NoteValue.Eighth);
    }

    [Fact]
    public void WithThreeRestsFollowedByOneNote_UpscalesToTwoRestOneNote()
    {
        // Arrange
        var noteGroups = new List<NoteGroup>
        {
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(Beat.Snare, NoteValue.Sixteenth) }
        };

        // Act
        var result = RecordingService.UpscaleNotes(noteGroups);

        // Assert
        result.Count.ShouldBe(3);
        result[0].Count.ShouldBe(1);
        result[1].Count.ShouldBe(1);
        result[2].Count.ShouldBe(1);
        result[0][0].ShouldHaveBeatAndValue(Beat.Rest, NoteValue.Eighth);
        result[1][0].ShouldHaveBeatAndValue(Beat.Rest, NoteValue.Sixteenth);
        result[2][0].ShouldHaveBeatAndValue(Beat.Snare, NoteValue.Sixteenth);
    }

    [Fact]
    public void WithMultipleBeatsInNoteGroup_PreservesAllBeats()
    {
        // Arrange
        var noteGroups = new List<NoteGroup>
        {
            new()
            {
                new Note(Beat.Bass, NoteValue.Sixteenth),
                new Note(Beat.HiHat, NoteValue.Sixteenth)
            },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) },
            new() { new Note(Beat.HiHat, NoteValue.Sixteenth) },
            new() { new Note(Beat.Rest, NoteValue.Sixteenth) }
        };

        // Act
        var result = RecordingService.UpscaleNotes(noteGroups);

        // Assert
        result.Count.ShouldBe(2);

        // First group should have two notes with eighth value
        result[0].Count.ShouldBe(2);
        result[0][0].Beat.ShouldBe(Beat.Bass);
        result[0][0].Value.ShouldBe(NoteValue.Eighth);
        result[0][1].Beat.ShouldBe(Beat.HiHat);
        result[0][1].Value.ShouldBe(NoteValue.Eighth);

        // Second group should be just HiHat with eighth value
        result[1].Count.ShouldBe(1);
        result[1][0].Beat.ShouldBe(Beat.HiHat);
        result[1][0].Value.ShouldBe(NoteValue.Eighth);
    }

    [Fact]
    public void WithFourConsecutiveMultipleBeats_KeepsThemDistinct()
    {
        // Arrange
        var noteGroups = new List<NoteGroup>
        {
            new()
            {
                new Note(Beat.Bass, NoteValue.Sixteenth),
                new Note(Beat.HiHat, NoteValue.Sixteenth)
            },
            new() { new Note(Beat.HiHat, NoteValue.Sixteenth) },
            new()
            {
                new Note(Beat.Snare, NoteValue.Sixteenth),
                new Note(Beat.HiHat, NoteValue.Sixteenth)
            },
            new() { new Note(Beat.HiHat, NoteValue.Sixteenth) }
        };

        // Act
        var result = RecordingService.UpscaleNotes(noteGroups);

        // Assert
        result.Count.ShouldBe(4);

        // Verify the counts and notes in each group
        result[0].Count.ShouldBe(2);
        result[0][0].Beat.ShouldBe(Beat.Bass);
        result[0][1].Beat.ShouldBe(Beat.HiHat);

        result[1].Count.ShouldBe(1);
        result[1][0].Beat.ShouldBe(Beat.HiHat);

        result[2].Count.ShouldBe(2);
        result[2][0].Beat.ShouldBe(Beat.Snare);
        result[2][1].Beat.ShouldBe(Beat.HiHat);

        result[3].Count.ShouldBe(1);
        result[3][0].Beat.ShouldBe(Beat.HiHat);

        // All should be sixteenth notes (not upscaled)
        for (var i = 0; i < 4; i++)
            foreach (var note in result[i])
                note.Value.ShouldBe(NoteValue.Sixteenth);
    }
}