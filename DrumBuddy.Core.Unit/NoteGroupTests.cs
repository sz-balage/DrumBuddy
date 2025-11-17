using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using Shouldly;

namespace DrumBuddy.Core.Unit;

public class WhenNoteGroupIsCreated
{
    [Fact]
    public void WithNoNotes_ShouldBeARest()
    {
        // Act
        var noteGroup = new NoteGroup();

        // Assert
        noteGroup.IsRest.ShouldBeTrue();
    }

    [Fact]
    public void WithOnlyARestNote_ShouldBeARest()
    {
        // Arrange
        var notes = new List<Note> { new(Drum.Rest, NoteValue.Quarter) };

        // Act
        var noteGroup = new NoteGroup(notes);

        // Assert
        noteGroup.IsRest.ShouldBeTrue();
    }

    [Fact]
    public void WithValidNotes_ShouldNotBeARest()
    {
        // Arrange
        var notes = new List<Note> { new(Drum.Kick, NoteValue.Quarter) };

        // Act
        var noteGroup = new NoteGroup(notes);

        // Assert
        noteGroup.IsRest.ShouldBeFalse();
    }

    [Fact]
    public void WithListConstructor_ShouldLoadAllNotes()
    {
        // Arrange
        var notes = new List<Note>
        {
            new(Drum.Kick, NoteValue.Quarter),
            new(Drum.Snare, NoteValue.Quarter)
        };

        // Act
        var noteGroup = new NoteGroup(notes);

        // Assert
        noteGroup.Count.ShouldBe(2);
    }

    [Fact]
    public void WithEnumerableConstructor_ShouldLoadAllNotes()
    {
        // Arrange
        var notes = new[]
        {
            new Note(Drum.Kick, NoteValue.Quarter),
            new Note(Drum.Snare, NoteValue.Quarter)
        }.AsEnumerable();

        // Act
        var noteGroup = new NoteGroup(notes);

        // Assert
        noteGroup.Count.ShouldBe(2);
    }
}

public class WhenNoteGroupExceedsMaxSize
{
    [Fact]
    public void WithMoreThanFourNotes_ShouldTruncateToMaxSize()
    {
        // Arrange
        var notes = new List<Note>
        {
            new(Drum.Kick, NoteValue.Quarter),
            new(Drum.Snare, NoteValue.Quarter),
            new(Drum.Tom1, NoteValue.Quarter),
            new(Drum.Tom2, NoteValue.Quarter),
            new(Drum.HiHat, NoteValue.Quarter) // This should be ignored
        };

        // Act
        var noteGroup = new NoteGroup(notes);

        // Assert
        noteGroup.Count.ShouldBe(4);
        noteGroup[3].Drum.ShouldBe(Drum.Tom2);
    }

    [Fact]
    public void WhenAddingBeyondMaxSize_ShouldNotAddNote()
    {
        // Arrange
        var noteGroup = new NoteGroup(new List<Note>
        {
            new(Drum.Kick, NoteValue.Quarter),
            new(Drum.Snare, NoteValue.Quarter),
            new(Drum.Tom1, NoteValue.Quarter),
            new(Drum.Tom2, NoteValue.Quarter)
        });

        // Act
        noteGroup.Add(new Note(Drum.HiHat, NoteValue.Quarter));

        // Assert
        noteGroup.Count.ShouldBe(4);
    }

    [Fact]
    public void WhenAddingWithinMaxSize_ShouldAddNote()
    {
        // Arrange
        var noteGroup = new NoteGroup();

        // Act
        noteGroup.Add(new Note(Drum.Kick, NoteValue.Quarter));
        noteGroup.Add(new Note(Drum.Snare, NoteValue.Quarter));

        // Assert
        noteGroup.Count.ShouldBe(2);
    }
}

public class WhenNoteGroupIsQueried
{
    [Fact]
    public void WithContainsCheck_ShouldReturnTrueForExistingDrum()
    {
        // Arrange
        var notes = new List<Note>
        {
            new(Drum.Kick, NoteValue.Quarter),
            new(Drum.Snare, NoteValue.Quarter)
        };
        var noteGroup = new NoteGroup(notes);

        // Act & Assert
        noteGroup.Contains(Drum.Kick).ShouldBeTrue();
        noteGroup.Contains(Drum.Snare).ShouldBeTrue();
    }

    [Fact]
    public void WithContainsCheck_ShouldReturnFalseForNonExistingDrum()
    {
        // Arrange
        var notes = new List<Note> { new(Drum.Kick, NoteValue.Quarter) };
        var noteGroup = new NoteGroup(notes);

        // Act & Assert
        noteGroup.Contains(Drum.Snare).ShouldBeFalse();
    }

    [Fact]
    public void WhenGettingValue_ShouldReturnFirstNoteValue()
    {
        // Arrange
        var notes = new List<Note>
        {
            new(Drum.Kick, NoteValue.Quarter),
            new(Drum.Snare, NoteValue.Eighth)
        };
        var noteGroup = new NoteGroup(notes);

        // Act & Assert
        noteGroup.Value.ShouldBe(NoteValue.Quarter);
    }
}

public class WhenNoteGroupValueIsChanged
{
    [Theory]
    [InlineData(NoteValue.Quarter)]
    [InlineData(NoteValue.Eighth)]
    [InlineData(NoteValue.Sixteenth)]
    public void ToAllNoteValues_ShouldChangeSuccessfully(NoteValue newValue)
    {
        // Arrange
        var notes = new List<Note>
        {
            new(Drum.Kick, NoteValue.Quarter),
            new(Drum.Snare, NoteValue.Quarter)
        };
        var noteGroup = new NoteGroup(notes);

        // Act
        var newGroup = noteGroup.ChangeValues(newValue);

        // Assert
        newGroup.Value.ShouldBe(newValue);
        newGroup.ShouldAllBe(note => note.Value == newValue);
    }

    [Fact]
    public void ShouldCreateNewGroupWithoutModifyingOriginal()
    {
        // Arrange
        var notes = new List<Note> { new(Drum.Kick, NoteValue.Quarter) };
        var noteGroup = new NoteGroup(notes);

        // Act
        var newGroup = noteGroup.ChangeValues(NoteValue.Eighth);

        // Assert
        noteGroup.Value.ShouldBe(NoteValue.Quarter);
        newGroup.Value.ShouldBe(NoteValue.Eighth);
    }
}