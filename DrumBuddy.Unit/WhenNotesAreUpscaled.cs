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
        var notes = new List<Note>
        {
            new(beat, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth)
        };

        // Act
        var result = RecordingService.UpscaleNotes(notes);

        // Assert
        result.Count.ShouldBe(1);
        result[0].ShouldHaveBeatAndValue(beat, NoteValue.Quarter);
    }

    [Theory]
    [InlineData(Beat.Snare, Beat.Snare)]
    [InlineData(Beat.Bass, Beat.Bass)]
    [InlineData(Beat.Snare, Beat.Bass)]
    public void WithFourConsecutiveNotes_KeepsThemDistinct(Beat beat1, Beat beat2)
    {
        // Arrange
        var notes = new List<Note>
        {
            new(beat1, NoteValue.Sixteenth),
            new(beat2, NoteValue.Sixteenth),
            new(beat1, NoteValue.Sixteenth),
            new(beat2, NoteValue.Sixteenth)
        };

        // Act
        var result = RecordingService.UpscaleNotes(notes);

        // Assert
        result.Count.ShouldBe(4);
        for (var i = 0; i < 4; i++)
        {
            if (i == 0 || i == 2)
            {
                result[i].ShouldHaveBeatAndValue(beat1, NoteValue.Sixteenth);
            }
            else
            {
                result[i].ShouldHaveBeatAndValue(beat2, NoteValue.Sixteenth);
            }
        }
    }

    [Fact]
    public void WithConsecutiveRests_UpscalesNotes()
    {
        // Arrange
        var notes = new List<Note>
        {
            new(Beat.Snare, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth),
            new(Beat.Snare, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth)
        };

        // Act
        var result = RecordingService.UpscaleNotes(notes);

        // Assert
        result.Count.ShouldBe(2);
        result[0].ShouldHaveBeatAndValue(Beat.Snare, NoteValue.Eighth);
        result[1].ShouldHaveBeatAndValue(Beat.Snare, NoteValue.Eighth);
    }


    [Theory]
    [InlineData(Beat.Snare, Beat.Snare)]
    [InlineData(Beat.Bass, Beat.Bass)]
    [InlineData(Beat.Snare, Beat.Bass)]
    public void WithOneNoteFollowedByTwoRestsAndOneNote_UpscalesToNoteRestNote(Beat beat1, Beat beat2)
    {
        var notes = new List<Note>
        {
            new(beat1, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth),
            new(beat2, NoteValue.Sixteenth)
        };
        var result = RecordingService.UpscaleNotes(notes);

        result.Count.ShouldBe(3);
        result[0].ShouldHaveBeatAndValue(beat1, NoteValue.Eighth);
        result[1].ShouldHaveBeatAndValue(Beat.Rest, NoteValue.Sixteenth);
        result[2].ShouldHaveBeatAndValue(beat2, NoteValue.Sixteenth);
    }

    [Fact]
    public void WithFourRests_UpscalesToOneQuarterRest()
    {
        // Arrange
        var notes = new List<Note>
        {
            new(Beat.Rest, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth)
        };

        // Act
        var result = RecordingService.UpscaleNotes(notes);

        // Assert
        result.Count.ShouldBe(1);
        result[0].ShouldHaveBeatAndValue(Beat.Rest, NoteValue.Quarter);
    }

    [Fact]
    public void WithTwoRestsFollowedByNoteAndRest_UpscalesToOneRestOneNote()
    {
        // Arrange
        var notes = new List<Note>
        {
            new(Beat.Rest, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth),
            new(Beat.Snare, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth)
        };

        // Act
        var result = RecordingService.UpscaleNotes(notes);

        // Assert
        result.Count.ShouldBe(2);
        result[0].ShouldHaveBeatAndValue(Beat.Rest, NoteValue.Eighth);
        result[1].ShouldHaveBeatAndValue(Beat.Snare, NoteValue.Eighth);
    }

    [Fact]
    public void WithThreeRestsFollowedByOneNote_UpscalesToTwoRestOneNote()
    {
        // Arrange
        var notes = new List<Note>
        {
            new(Beat.Rest, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth),
            new(Beat.Rest, NoteValue.Sixteenth),
            new(Beat.Snare, NoteValue.Sixteenth)
        };

        // Act
        var result = RecordingService.UpscaleNotes(notes);

        // Assert
        result.Count.ShouldBe(3);
        result[0].ShouldHaveBeatAndValue(Beat.Rest, NoteValue.Eighth);
        result[1].ShouldHaveBeatAndValue(Beat.Rest, NoteValue.Sixteenth);
        result[2].ShouldHaveBeatAndValue(Beat.Snare, NoteValue.Sixteenth);
    }

}