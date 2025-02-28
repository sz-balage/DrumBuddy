using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Enums;
using Shouldly;

namespace DrumBuddy.Core.Unit;

public class WhenNotesAreUpscaled
{
       [Fact]
        public void WithOneNoteFollowedByThreeRests_UpscalesToQuarter()
        {
            // Arrange
            var notes = new List<Note>
            {
                new Note(Beat.Snare, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
            };

            // Act
            var result = RecordingService.UpscaleNotes(notes);

            // Assert
            result.Count.ShouldBe(1);
            result[0].ShouldHaveBeatAndValue(Beat.Snare, NoteValue.Quarter);
        }

        [Fact]
        public void WithFourConsecutiveSnares_KeepsThemDistinct()
        {
            // Arrange
            var notes = new List<Note>
            {
                new Note(Beat.Snare, NoteValue.Sixteenth),
                new Note(Beat.Snare, NoteValue.Sixteenth),
                new Note(Beat.Snare, NoteValue.Sixteenth),
                new Note(Beat.Snare, NoteValue.Sixteenth),
            };

            // Act
            var result = RecordingService.UpscaleNotes(notes);

            // Assert
            result.Count.ShouldBe(4);
            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(Beat.Snare, result[i].Beat);
                Assert.Equal(NoteValue.Sixteenth, result[i].Value);
            }
        }

        [Fact]
        public void WithConsecutiveRests_UpscalesNotes()
        {
            // Arrange
            var notes = new List<Note>
            {
                new Note(Beat.Snare, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
                new Note(Beat.Snare, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
            };

            // Act
            var result = RecordingService.UpscaleNotes(notes);

            // Assert
            result.Count.ShouldBe(2);
            result[0].ShouldHaveBeatAndValue(Beat.Snare, NoteValue.Eighth);
            result[1].ShouldHaveBeatAndValue(Beat.Snare, NoteValue.Eighth);
        }


        [Fact]
        public void WithOneNoteFollowedByTwoRestsAndOneNote_UpscalesToNoteRestNote()
        {
            var notes = new List<Note>
            {
                new Note(Beat.Snare, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
                new Note(Beat.Snare, NoteValue.Sixteenth),
            };
            var result = RecordingService.UpscaleNotes(notes);

            result.Count.ShouldBe(3);
            result[0].ShouldHaveBeatAndValue(Beat.Snare, NoteValue.Eighth);
            result[1].ShouldHaveBeatAndValue(Beat.Rest, NoteValue.Sixteenth);
            result[2].ShouldHaveBeatAndValue(Beat.Snare, NoteValue.Sixteenth);
        }

        [Fact]
        public void WithFourRests_UpscalesToOneQuarterRest()
        {
            // Arrange
            var notes = new List<Note>
            {
                new Note(Beat.Rest, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
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
                new Note(Beat.Rest, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
                new Note(Beat.Snare, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
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
                new Note(Beat.Rest, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
                new Note(Beat.Rest, NoteValue.Sixteenth),
                new Note(Beat.Snare, NoteValue.Sixteenth),
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