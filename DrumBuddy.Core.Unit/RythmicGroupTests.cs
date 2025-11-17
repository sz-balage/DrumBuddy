using System.Collections.Immutable;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using Shouldly;

namespace DrumBuddy.Core.Unit;

public class WhenRythmicGroupIsCreated
{
    [Fact]
    public void WithEmptyNoteGroups_ShouldBeEmpty()
    {
        // Arrange
        var noteGroups = ImmutableArray<NoteGroup>.Empty;

        // Act
        var rythmicGroup = new RythmicGroup(noteGroups);

        // Assert
        rythmicGroup.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void WithValidNoteGroups_ShouldNotBeEmpty()
    {
        // Arrange
        var noteGroups = ImmutableArray.Create(
            new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) })
        );

        // Act
        var rythmicGroup = new RythmicGroup(noteGroups);

        // Assert
        rythmicGroup.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void WithAllRestNoteGroups_ShouldBeEmpty()
    {
        // Arrange
        var noteGroups = ImmutableArray.Create(
            new NoteGroup(),
            new NoteGroup(new List<Note> { new(Drum.Rest, NoteValue.Quarter) })
        );

        // Act
        var rythmicGroup = new RythmicGroup(noteGroups);

        // Assert
        rythmicGroup.IsEmpty.ShouldBeTrue();
    }
}

public class WhenRythmicGroupsAreCompared
{
    [Fact]
    public void WithIdenticalNoteGroups_ShouldBeEqual()
    {
        // Arrange
        var noteGroups1 = ImmutableArray.Create(
            new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) })
        );
        var noteGroups2 = ImmutableArray.Create(
            new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) })
        );
        var group1 = new RythmicGroup(noteGroups1);
        var group2 = new RythmicGroup(noteGroups2);

        // Act & Assert
        group1.ShouldBe(group2);
    }

    [Fact]
    public void WithDifferentGroupCounts_ShouldNotBeEqual()
    {
        // Arrange
        var noteGroups1 = ImmutableArray.Create(
            new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) }),
            new NoteGroup(new List<Note> { new(Drum.Snare, NoteValue.Quarter) })
        );
        var noteGroups2 = ImmutableArray.Create(
            new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) })
        );
        var group1 = new RythmicGroup(noteGroups1);
        var group2 = new RythmicGroup(noteGroups2);

        // Act & Assert
        group1.ShouldNotBe(group2);
    }

    [Fact]
    public void WithDifferentNotes_ShouldNotBeEqual()
    {
        // Arrange
        var noteGroups1 = ImmutableArray.Create(
            new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) })
        );
        var noteGroups2 = ImmutableArray.Create(
            new NoteGroup(new List<Note> { new(Drum.Snare, NoteValue.Quarter) })
        );
        var group1 = new RythmicGroup(noteGroups1);
        var group2 = new RythmicGroup(noteGroups2);

        // Act & Assert
        group1.ShouldNotBe(group2);
    }

    [Fact]
    public void WithDifferentNoteCountsInGroup_ShouldNotBeEqual()
    {
        // Arrange
        var noteGroups1 = ImmutableArray.Create(
            new NoteGroup(new List<Note>
            {
                new(Drum.Kick, NoteValue.Quarter),
                new(Drum.Snare, NoteValue.Quarter)
            })
        );
        var noteGroups2 = ImmutableArray.Create(
            new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) })
        );
        var group1 = new RythmicGroup(noteGroups1);
        var group2 = new RythmicGroup(noteGroups2);

        // Act & Assert
        group1.ShouldNotBe(group2);
    }

    [Fact]
    public void WithComplexMultipleNoteGroupsSameDrums_ShouldBeEqual()
    {
        // Arrange
        var noteGroups1 = ImmutableArray.Create(
            new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) }),
            new NoteGroup(new List<Note> { new(Drum.Snare, NoteValue.Quarter) }),
            new NoteGroup(new List<Note> { new(Drum.HiHat, NoteValue.Eighth) })
        );
        var noteGroups2 = ImmutableArray.Create(
            new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) }),
            new NoteGroup(new List<Note> { new(Drum.Snare, NoteValue.Quarter) }),
            new NoteGroup(new List<Note> { new(Drum.HiHat, NoteValue.Eighth) })
        );
        var group1 = new RythmicGroup(noteGroups1);
        var group2 = new RythmicGroup(noteGroups2);

        // Act & Assert
        group1.ShouldBe(group2);
    }
}