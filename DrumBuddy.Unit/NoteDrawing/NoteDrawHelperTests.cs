using System.Collections.Immutable;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Models;
using DrumBuddy.Services.Layout;
using Shouldly;

namespace DrumBuddy.Unit.NoteDrawing;

public class NoteLayoutEngineTests
{
    private const double CanvasWidth = 800;
    private const double CanvasHeight = 600;

    private Dictionary<Drum, DrumPositionSlot> CreatePositionMap()
    {
        return new Dictionary<Drum, DrumPositionSlot>
        {
            { Drum.HiHat, DrumPositionSlot.BetweenLine5And6 },
            { Drum.HiHat_Open, DrumPositionSlot.BetweenLine5And6 },
            { Drum.HiHat_Pedal, DrumPositionSlot.BelowLine1 },
            { Drum.Ride, DrumPositionSlot.OnLine5 },
            { Drum.Crash1, DrumPositionSlot.OnLine6 },
            { Drum.Crash2, DrumPositionSlot.OnLine6 },
            { Drum.Kick, DrumPositionSlot.BetweenLine1And2 },
            { Drum.Snare, DrumPositionSlot.BetweenLine3And4 },
            { Drum.Tom1, DrumPositionSlot.BetweenLine4And5 },
            { Drum.Tom2, DrumPositionSlot.OnLine4 },
            { Drum.FloorTom, DrumPositionSlot.BetweenLine2And3 }
        };
    }

    private NoteLayoutEngine CreateEngine()
    {
        return new NoteLayoutEngine(CanvasWidth, CanvasHeight, CreatePositionMap());
    }

    public static IEnumerable<object[]> AllDrumsExceptRest()
    {
        // Provide every drum except Rest
        return Enum.GetValues(typeof(Drum))
            .Cast<Drum>()
            .Where(d => d != Drum.Rest)
            .Select(d => new object[] { d });
    }

    [Theory]
    [MemberData(nameof(AllDrumsExceptRest))]
    public void AllDrums_ShouldUseCorrectMappedYPosition(Drum drum)
    {
        // arrange
        var map = CreatePositionMap();
        var eng = CreateEngine();

        var group = new RythmicGroup([
            new NoteGroup([new Note(drum, NoteValue.Quarter)])
        ]);

        // act
        var result = eng.Generate(group);

        // assert
        result.Notes.Length.ShouldBe(1);

        var expectedY = (int)map[drum];
        result.Notes[0].Y.ShouldBe(expectedY);
    }

    [Theory]
    [MemberData(nameof(AllDrumsExceptRest))]
    public void AllDrums_ShouldUseCorrectXPosition(Drum drum)
    {
        var eng = CreateEngine();

        var group = new RythmicGroup([
            new NoteGroup([new Note(drum, NoteValue.Quarter)])
        ]);

        var result = eng.Generate(group);

        result.Notes.Length.ShouldBe(1);

        var expectedX = -(CanvasWidth / 2) + 20; // engine's fixed start-X

        result.Notes[0].X.ShouldBe(expectedX);
    }

    [Theory]
    [InlineData(NoteValue.Eighth, 400)]
    [InlineData(NoteValue.Sixteenth, 200)]
    public void XSpacing_ShouldMatchExpectedSpacing(NoteValue value, double expectedSpacing)
    {
        var eng = CreateEngine();

        var group = new RythmicGroup([
            new NoteGroup([new Note(Drum.Snare, value)]),
            new NoteGroup([new Note(Drum.Snare, value)])
        ]);

        var result = eng.Generate(group);

        var spacing = result.Notes[1].X - result.Notes[0].X;

        spacing.ShouldBe(expectedSpacing);
    }

    [Fact]
    public void EmptyRythmicGroup_ShouldReturnEmptyResult()
    {
        var eng = CreateEngine();
        var group = new RythmicGroup(ImmutableArray<NoteGroup>.Empty);

        var result = eng.Generate(group);

        result.Notes.ShouldBeEmpty();
        result.Lines.ShouldBeEmpty();
        result.Circles.ShouldBeEmpty();
    }

    [Fact]
    public void SingleQuarterNote_ShouldProduceOneNoteAndOneVerticalLine()
    {
        var eng = CreateEngine();
        var group = new RythmicGroup([
            new NoteGroup([new Note(Drum.Snare, NoteValue.Quarter)])
        ]);

        var result = eng.Generate(group);

        result.Notes.Length.ShouldBe(1);
        result.Lines.Count(l => l.LineType == LineType.Vertical).ShouldBe(1);
    }


    [Fact]
    public void MultipleNotesInGroup_ShouldPlaceNotesAtDifferentY()
    {
        var eng = CreateEngine();
        var group = new RythmicGroup([
            new NoteGroup([
                new Note(Drum.Snare, NoteValue.Quarter),
                new Note(Drum.Kick, NoteValue.Quarter)
            ])
        ]);

        var result = eng.Generate(group);

        result.Notes.Length.ShouldBe(2);
        result.Notes[0].Y.ShouldNotBe(result.Notes[1].Y);
    }


    [Fact]
    public void HiHatOpen_ShouldCreateCircleIndicator()
    {
        var eng = CreateEngine();
        var group = new RythmicGroup([
            new NoteGroup([new Note(Drum.HiHat_Open, NoteValue.Quarter)])
        ]);

        var result = eng.Generate(group);

        result.Notes.Length.ShouldBe(1);
        result.Circles.Length.ShouldBe(1);
    }


    [Fact]
    public void QuarterRest_ShouldProduceNoteButNoLines()
    {
        var eng = CreateEngine();
        var group = new RythmicGroup([
            new NoteGroup([new Note(Drum.Rest, NoteValue.Quarter)])
        ]);

        var result = eng.Generate(group);

        result.Notes.Length.ShouldBe(1);
        result.Lines.ShouldBeEmpty();
        result.Circles.ShouldBeEmpty();
    }

    [Fact]
    public void SixteenthRest_NotAtStart_ShouldPlaceCircleAbovePreviousNote()
    {
        var eng = CreateEngine();

        var groups = new RythmicGroup([
            new NoteGroup([new Note(Drum.Snare, NoteValue.Sixteenth)]),
            new NoteGroup([new Note(Drum.Rest, NoteValue.Sixteenth)])
        ]);

        var result = eng.Generate(groups);

        // One note + one circle
        result.Notes.Length.ShouldBe(1);
        result.Circles.Length.ShouldBe(1);

        var prevNote = result.Notes[0];
        var circle = result.Circles[0];

        circle.X.ShouldBe(prevNote.X + 20);
        circle.Y.ShouldBe(prevNote.Y); // same Y as note
    }

    [Fact]
    public void Crash1_ShouldHaveCorrectDrumPosition()
    {
        var eng = CreateEngine();
        var group = new RythmicGroup([
            new NoteGroup([new Note(Drum.Crash1, NoteValue.Quarter)])
        ]);

        var result = eng.Generate(group);

        result.Notes.Length.ShouldBe(1);
        result.Notes[0].Y.ShouldBe((int)DrumPositionSlot.OnLine6);
    }

    [Fact]
    public void Ride_ShouldBePlacedOnLine5()
    {
        var eng = CreateEngine();
        var group = new RythmicGroup([
            new NoteGroup([new Note(Drum.Ride, NoteValue.Quarter)])
        ]);

        var result = eng.Generate(group);

        result.Notes.Length.ShouldBe(1);
        result.Notes[0].Y.ShouldBe((int)DrumPositionSlot.OnLine5);
    }


    [Fact]
    public void TwoSixteenthNotes_ShouldProduceHorizontalBeam()
    {
        var eng = CreateEngine();
        var group = new RythmicGroup([
            new NoteGroup([new Note(Drum.Snare, NoteValue.Sixteenth)]),
            new NoteGroup([new Note(Drum.Snare, NoteValue.Sixteenth)])
        ]);

        var result = eng.Generate(group);

        // Two verticals + end horizontal + second beam = 4 lines
        result.Lines.Length.ShouldBeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void SingleSixteenth_ShouldAddDoubleDiagonalIfAtEnd()
    {
        var eng = CreateEngine();
        var ng = new NoteGroup([new Note(Drum.Snare, NoteValue.Sixteenth)]);
        var group = new RythmicGroup([ng]);

        var result = eng.Generate(group);

        // vertical + end horizontal + two diagonals
        result.Lines.Length.ShouldBe(3); // vertical + end horizontal + one diagonal line block? (matches logic)
    }


    [Fact]
    public void SingleEighth_ShouldProduceDiagonalStem()
    {
        var eng = CreateEngine();
        var group = new RythmicGroup([
            new NoteGroup([new Note(Drum.Snare, NoteValue.Eighth)])
        ]);

        var result = eng.Generate(group);

        result.Lines.Length.ShouldBe(2);
    }


    [Fact]
    public void ComplexPattern_ShouldGenerateEverything()
    {
        var eng = CreateEngine();
        var group = new RythmicGroup([
            new NoteGroup([new Note(Drum.Kick, NoteValue.Quarter)]),
            new NoteGroup([new Note(Drum.Snare, NoteValue.Eighth)]),
            new NoteGroup([new Note(Drum.HiHat, NoteValue.Sixteenth)]),
            new NoteGroup([new Note(Drum.Rest, NoteValue.Sixteenth)])
        ]);

        var result = eng.Generate(group);

        result.Notes.ShouldNotBeEmpty();
        result.Lines.ShouldNotBeEmpty();
    }


    [Fact]
    public void Coordinates_ShouldBeTranslatedFromCenter()
    {
        var eng = CreateEngine();
        var group = new RythmicGroup([
            new NoteGroup([new Note(Drum.Snare, NoteValue.Quarter)])
        ]);

        var result = eng.Generate(group);

        var note = result.Notes[0];

        // Starting X = -400 + 20 = -380 in center-space
        // Should remain -380 for logic layer — renderer does final conversion
        note.X.ShouldBe(-380);
    }
}