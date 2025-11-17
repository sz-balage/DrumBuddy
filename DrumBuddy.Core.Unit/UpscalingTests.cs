using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using Shouldly;

namespace DrumBuddy.Core.Unit;

public class UpscalingTests
{
    public class WhenNotesAreUpscaled
    {
        [Theory]
        [InlineData(Drum.Snare)]
        [InlineData(Drum.Kick)]
        [InlineData(Drum.HiHat)]
        [InlineData(Drum.Crash1)]
        [InlineData(Drum.Ride)]
        [InlineData(Drum.Tom1)]
        [InlineData(Drum.Tom2)]
        [InlineData(Drum.FloorTom)]
        public void WithOneNoteFollowedByThreeRests_UpscalesToQuarter(Drum drum)
        {
            // Arrange
            var noteGroups = new List<NoteGroup>
            {
                new() { new Note(drum, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) }
            };

            // Act
            var result = RecordingService.UpscaleNotes(noteGroups);

            // Assert
            result.Count.ShouldBe(1);
            result[0].Count.ShouldBe(1);
            result[0][0].ShouldHaveBeatAndValue(drum, NoteValue.Quarter);
            result[0].Value.ShouldBe(NoteValue.Quarter);
        }

        [Theory]
        [InlineData(Drum.Snare, Drum.Snare)]
        [InlineData(Drum.Kick, Drum.Kick)]
        [InlineData(Drum.Snare, Drum.Kick)]
        public void WithFourConsecutiveNotes_KeepsThemDistinct(Drum beat1, Drum beat2)
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
                {
                    result[i][0].ShouldHaveBeatAndValue(beat1, NoteValue.Sixteenth);
                    result[i].Value.ShouldBe(NoteValue.Sixteenth);
                }
                else
                {
                    result[i][0].ShouldHaveBeatAndValue(beat2, NoteValue.Sixteenth);
                    result[i].Value.ShouldBe(NoteValue.Sixteenth);
                }
            }
        }

        [Fact]
        public void WithConsecutiveRests_UpscalesNotes()
        {
            // Arrange
            var noteGroups = new List<NoteGroup>
            {
                new() { new Note(Drum.Snare, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(Drum.Snare, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) }
            };

            // Act
            var result = RecordingService.UpscaleNotes(noteGroups);

            // Assert
            result.Count.ShouldBe(2);
            result[0].Count.ShouldBe(1);
            result[0].Value.ShouldBe(NoteValue.Eighth);
            result[1].Count.ShouldBe(1);
            result[1].Value.ShouldBe(NoteValue.Eighth);
            result[0][0].ShouldHaveBeatAndValue(Drum.Snare, NoteValue.Eighth);
            result[1][0].ShouldHaveBeatAndValue(Drum.Snare, NoteValue.Eighth);
        }

        [Theory]
        [InlineData(Drum.Snare, Drum.Snare)]
        [InlineData(Drum.Kick, Drum.Kick)]
        [InlineData(Drum.Snare, Drum.Kick)]
        public void WithOneNoteFollowedByTwoRestsAndOneNote_UpscalesToNoteRestNote(Drum beat1, Drum beat2)
        {
            var noteGroups = new List<NoteGroup>
            {
                new() { new Note(beat1, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(beat2, NoteValue.Sixteenth) }
            };

            var result = RecordingService.UpscaleNotes(noteGroups);

            result.Count.ShouldBe(3);
            result[0].Count.ShouldBe(1);
            result[0].Value.ShouldBe(NoteValue.Eighth);
            result[1].Count.ShouldBe(1);
            result[1].Value.ShouldBe(NoteValue.Sixteenth);
            result[2].Count.ShouldBe(1);
            result[2].Value.ShouldBe(NoteValue.Sixteenth);
            result[0][0].ShouldHaveBeatAndValue(beat1, NoteValue.Eighth);
            result[1][0].ShouldHaveBeatAndValue(Drum.Rest, NoteValue.Sixteenth);
            result[2][0].ShouldHaveBeatAndValue(beat2, NoteValue.Sixteenth);
        }

        [Fact]
        public void WithFourRests_UpscalesToOneQuarterRest()
        {
            // Arrange
            var noteGroups = new List<NoteGroup>
            {
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) }
            };

            // Act
            var result = RecordingService.UpscaleNotes(noteGroups);

            // Assert
            result.Count.ShouldBe(1);
            result[0].Count.ShouldBe(1);
            result[0].Value.ShouldBe(NoteValue.Quarter);
            result[0][0].ShouldHaveBeatAndValue(Drum.Rest, NoteValue.Quarter);
        }

        [Fact]
        public void WithTwoRestsFollowedByNoteAndRest_UpscalesToOneRestOneNote()
        {
            // Arrange
            var noteGroups = new List<NoteGroup>
            {
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(Drum.Snare, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) }
            };

            // Act
            var result = RecordingService.UpscaleNotes(noteGroups);

            // Assert
            result.Count.ShouldBe(2);
            result[0].Count.ShouldBe(1);
            result[0].Value.ShouldBe(NoteValue.Eighth);
            result[1].Count.ShouldBe(1);
            result[1].Value.ShouldBe(NoteValue.Eighth);
            result[0][0].ShouldHaveBeatAndValue(Drum.Rest, NoteValue.Eighth);
            result[1][0].ShouldHaveBeatAndValue(Drum.Snare, NoteValue.Eighth);
        }

        [Fact]
        public void WithThreeRestsFollowedByOneNote_UpscalesToTwoRestOneNote()
        {
            // Arrange
            var noteGroups = new List<NoteGroup>
            {
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(Drum.Snare, NoteValue.Sixteenth) }
            };

            // Act
            var result = RecordingService.UpscaleNotes(noteGroups);

            // Assert
            result.Count.ShouldBe(3);
            result[0].Count.ShouldBe(1);
            result[0].Value.ShouldBe(NoteValue.Eighth);
            result[1].Count.ShouldBe(1);
            result[1].Value.ShouldBe(NoteValue.Sixteenth);
            result[2].Count.ShouldBe(1);
            result[2].Value.ShouldBe(NoteValue.Sixteenth);
            result[0][0].ShouldHaveBeatAndValue(Drum.Rest, NoteValue.Eighth);
            result[1][0].ShouldHaveBeatAndValue(Drum.Rest, NoteValue.Sixteenth);
            result[2][0].ShouldHaveBeatAndValue(Drum.Snare, NoteValue.Sixteenth);
        }

        [Fact]
        public void WithMultipleBeatsInNoteGroup_PreservesAllBeats()
        {
            // Arrange
            var noteGroups = new List<NoteGroup>
            {
                new()
                {
                    new Note(Drum.Kick, NoteValue.Sixteenth),
                    new Note(Drum.HiHat, NoteValue.Sixteenth)
                },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) },
                new() { new Note(Drum.HiHat, NoteValue.Sixteenth) },
                new() { new Note(Drum.Rest, NoteValue.Sixteenth) }
            };

            // Act
            var result = RecordingService.UpscaleNotes(noteGroups);

            // Assert
            result.Count.ShouldBe(2);

            // First group should have two notes with eighth value
            result[0].Count.ShouldBe(2);
            result[0].Value.ShouldBe(NoteValue.Eighth);
            result[0][0].Drum.ShouldBe(Drum.Kick);
            result[0][0].Value.ShouldBe(NoteValue.Eighth);
            result[0][1].Drum.ShouldBe(Drum.HiHat);
            result[0][1].Value.ShouldBe(NoteValue.Eighth);

            // Second group should be just HiHat with eighth value
            result[1].Count.ShouldBe(1);
            result[1].Value.ShouldBe(NoteValue.Eighth);
            result[1][0].Drum.ShouldBe(Drum.HiHat);
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
                    new Note(Drum.Kick, NoteValue.Sixteenth),
                    new Note(Drum.HiHat, NoteValue.Sixteenth)
                },
                new() { new Note(Drum.HiHat, NoteValue.Sixteenth) },
                new()
                {
                    new Note(Drum.Snare, NoteValue.Sixteenth),
                    new Note(Drum.HiHat, NoteValue.Sixteenth)
                },
                new() { new Note(Drum.HiHat, NoteValue.Sixteenth) }
            };

            // Act
            var result = RecordingService.UpscaleNotes(noteGroups);

            // Assert
            result.Count.ShouldBe(4);

            // Verify the counts and notes in each group
            result[0].Count.ShouldBe(2);
            result[0].Value.ShouldBe(NoteValue.Sixteenth);
            result[0][0].Drum.ShouldBe(Drum.Kick);
            result[0][1].Drum.ShouldBe(Drum.HiHat);

            result[1].Count.ShouldBe(1);
            result[1].Value.ShouldBe(NoteValue.Sixteenth);
            result[1][0].Drum.ShouldBe(Drum.HiHat);

            result[2].Count.ShouldBe(2);
            result[2].Value.ShouldBe(NoteValue.Sixteenth);
            result[2][0].Drum.ShouldBe(Drum.Snare);
            result[2][1].Drum.ShouldBe(Drum.HiHat);

            result[3].Count.ShouldBe(1);
            result[3].Value.ShouldBe(NoteValue.Sixteenth);
            result[3][0].Drum.ShouldBe(Drum.HiHat);

            // All should be sixteenth notes (not upscaled)
            for (var i = 0; i < 4; i++)
                foreach (var note in result[i])
                    note.Value.ShouldBe(NoteValue.Sixteenth);
        }
    }
}