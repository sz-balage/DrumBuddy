using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using Shouldly;

namespace DrumBuddy.Core.Unit;

public abstract class NoteTests
{
    public class WhenNotesAreCompared
    {
        [Fact]
        public void WithIdenticalDrumAndValue_ShouldBeEqual()
        {
            // Arrange
            var note1 = new Note(Drum.Kick, NoteValue.Quarter);
            var note2 = new Note(Drum.Kick, NoteValue.Quarter);

            // Assert
            note1.ShouldBe(note2);
        }

        [Fact]
        public void WithDifferentDrums_ShouldNotBeEqual()
        {
            // Arrange
            var note1 = new Note(Drum.Kick, NoteValue.Quarter);
            var note2 = new Note(Drum.Snare, NoteValue.Quarter);

            // Assert
            note1.ShouldNotBe(note2);
        }

        [Fact]
        public void WithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var note1 = new Note(Drum.Kick, NoteValue.Quarter);
            var note2 = new Note(Drum.Kick, NoteValue.Eighth);

            // Assert
            note1.ShouldNotBe(note2);
        }

        [Fact]
        public void WithSameValuesInCollection_ShouldHaveEqualHashCode()
        {
            // Arrange
            var note1 = new Note(Drum.Kick, NoteValue.Quarter);
            var note2 = new Note(Drum.Kick, NoteValue.Quarter);

            // Act & Assert
            note1.GetHashCode().ShouldBe(note2.GetHashCode());
        }
    }
}