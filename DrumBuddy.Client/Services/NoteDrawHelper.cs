using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Avalonia;
using DrumBuddy.Client.Models;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Enums;

namespace DrumBuddy.Client.Services;

public class NoteDrawHelper
{
    private const string BaseNotationPath = "avares://DrumBuddy.Client/Assets/Notation/";
    private const string ImageExtension = ".png";

    private const int LineThickness = 2;

    private static readonly Size NoteHeadSize = new(24, 20);
    private static readonly Size NoteHeadWithLineSize = new(28, 20);

    private static readonly Size QuarterRestImageSize = new(60, 60);
    private static readonly Size EighthRestImageSize = new(40, 40);
    private static readonly Size SixteenthRestImageSize = new(60, 60);
    private static readonly Size CircleSize = new(8, 8);

    private double FromCentreBasedToTopLeftCoordinateX(double coordinateX)
    {
        return coordinateX + _canvasWidth / 2;
    }

    private double FromCentreBasedToTopLeftCoordinateY(double coordinateY)
    {
        return coordinateY + _canvasHeight / 2;
    }

    private static bool AreOneDrumAway(Drum drum, Drum otherDrum)
    {
        return Math.Abs(GetYPositionForDrum(drum) - GetYPositionForDrum(otherDrum)) == NoteHeadSize.Height / 2;
    }

    private LineAndStroke GetLineForNoteGroup(NoteGroup noteGroup, double xPosition)
    {
        //get start and endpoint for each note groups line -> the start point is the y position of the lowest note in the group, the endpoint is the y position of the highest note in the group + 3 * notehead height
        var lowestY = GetYPositionForDrum(noteGroup.First().Drum);
        var highestY = GetYPositionForDrum(noteGroup.Last().Drum);
        var startPointY = FromCentreBasedToTopLeftCoordinateY(lowestY); // + 20; // 85;
        var endPointY = FromCentreBasedToTopLeftCoordinateY(highestY - 3 * NoteHeadSize.Height); // -15 - 60;//-15;// 
        return new LineAndStroke(new Point(FromCentreBasedToTopLeftCoordinateX(xPosition) + 12, startPointY),
            new Point(FromCentreBasedToTopLeftCoordinateX(xPosition) + 12, endPointY), LineThickness);
    }

    /// <summary>
    ///     Get on/or between which line the drum should be drawn.
    /// </summary>
    /// <param name="drum">Drum to be drawn.</param>
    /// <returns></returns>
    private static double GetYPositionForDrum(Drum drum)
    {
        return drum switch
        {
            Drum.Kick => 65, // between line 1 and 2
            Drum.Snare => 25, // between line 3 and 4
            Drum.FloorTom => 45, // between line 2 and 3
            Drum.Tom1 => 5, // between line 4 and 5
            Drum.Tom2 => 15, // on line 4
            Drum.Ride => -5, // on line 5
            Drum.HiHat => -15, // above line 5 (between line 5 and the invisible line 6)
            Drum.Crash1 => -25, // on line 6
            _ => 35
        };
    }

    private static (Uri Path, Size ImageSize) NoteHeadImagePathAndSize(Note note)
    {
        return note.Drum switch
        {
            Drum.HiHat or Drum.Ride => (new Uri(BaseNotationPath + "note_head_x" + ImageExtension), NoteHeadSize),
            Drum.Crash1 => (new Uri(BaseNotationPath + "note_head_x_line" + ImageExtension), NoteHeadWithLineSize),
            Drum.Rest => note.Value switch
            {
                NoteValue.Quarter => (new Uri(BaseNotationPath + "quarter_rest" + ImageExtension),
                    QuarterRestImageSize),
                NoteValue.Eighth => (new Uri(BaseNotationPath + "eighth_rest" + ImageExtension), EighthRestImageSize),
                _ => (new Uri(BaseNotationPath + "sixteenth_rest" + ImageExtension), SixteenthRestImageSize)
            },
            _ => (new Uri(BaseNotationPath + "note_head" + ImageExtension), NoteHeadSize)
        };
    }

    private static (Uri Path, Size ImageSize) GetCircleImagePathAndSize()
    {
        return (new Uri(BaseNotationPath + "circle" + ".png"), CircleSize);
    }

    private static double GetDisplacementForNoteValue(NoteValue beat, double noteGroupWidth)
    {
        return beat switch
        {
            NoteValue.Quarter => noteGroupWidth * 4,
            NoteValue.Eighth => noteGroupWidth * 2,
            _ => noteGroupWidth
        };
    }

    private readonly double _canvasWidth;
    private readonly double _canvasHeight;
    private readonly double _startingXPos;
    private readonly double _noteGroupWidth;

    public NoteDrawHelper(double canvasWidth, double canvasHeight)
    {
        _canvasWidth = canvasWidth;
        _canvasHeight = canvasHeight;
        _startingXPos = -1 * (_canvasWidth / 2) + 20; //since 0 is middle, we need to set it to left by decrementing it by half the width, and then add a minimal offset so it is not on the edge
        _noteGroupWidth = _canvasWidth / 4;
    }

    /// <summary>
    ///     Generates lines and note images for the given rythmic group.
    /// </summary>
    /// <param name="getCircleImage">Function for creating a circle image.</param>
    /// <param name="rythmicGroup">Containing the note groups to draw.</param>
    /// <param name="noteGroupWidth">Used for calculating displacement for each notegroup.</param>
    /// <param name="startingXPosition">Determines the position of the first notegroup horizontally.</param>
    /// <param name="getNoteImage">Function for creating a note image</param>
    /// <returns>The lines and images to draw.</returns>
    internal (ImmutableArray<NoteImageAndBounds> Images, ImmutableArray<LineAndStroke> LineAndStrokes)
        GenerateLinesAndNoteImages(
            Func<Note, Point, NoteImageAndBounds> getNoteImage,
            Func<Point, NoteImageAndBounds> getCircleImage,
            RythmicGroup rythmicGroup)
    {
        var linesAndStrokes = new List<LineAndStroke>(); //TODO: draw lines horizontally
        var images = new List<NoteImageAndBounds>();
        var noteGroups = rythmicGroup.NoteGroups;
        var x = _startingXPos;
        for (var i = 0; i < noteGroups.Length; i++)
        {
            var noteGroup = noteGroups[i];
            noteGroup.Sort((n1, n2) => GetYPositionForDrum(n2.Drum).CompareTo(GetYPositionForDrum(n1.Drum)));
            if (!noteGroup.IsRest)
                linesAndStrokes.Add(GetLineForNoteGroup(noteGroup, x));
            if (noteGroup is { IsRest: true, Value: NoteValue.Sixteenth } && i != 0)
            {
                var previousX = images[i - 1].Bounds.X;
                var previousY = images[i - 1].Bounds.Y;
                var point = new Point(previousX + 20, previousY);
                var circleImage =
                    getCircleImage(point); // TODO: find the rightmost note in notegroup and draw next to that
                images.Add(circleImage);
                x += GetDisplacementForNoteValue(noteGroup.Value,
                    _noteGroupWidth); 
                continue;
            }

            for (var j = 0; j < noteGroup.Count; j++)
            {
                var note = noteGroup[j];
                var y = GetYPositionForDrum(note.Drum);
                var point = new Point(x, y);
                if (j == 1 && AreOneDrumAway(note.Drum, noteGroup[0].Drum))
                    point = point.WithX(x + NoteHeadSize.Width);
                var noteImage = getNoteImage(note, point);
                images.Add(noteImage);
            }

            x += GetDisplacementForNoteValue(noteGroup.Value, _noteGroupWidth);
        }

        return ([..images], [..linesAndStrokes]);
    }

    public (ImmutableArray<NoteImageAndBounds> Images, ImmutableArray<LineAndStroke> LineAndStrokes)
        GetLinesAndImagesToDraw(RythmicGroup rythmicGroup)
    {
        Func<Note, Point, NoteImageAndBounds> getNoteImage = (note, point) =>
        {
            var noteHeadPathAndSize = NoteHeadImagePathAndSize(note);
            var noteImage = new NoteImageAndBounds(noteHeadPathAndSize.Path,
                new Rect(point, noteHeadPathAndSize.ImageSize));
            return noteImage;
        };
        Func<Point, NoteImageAndBounds> getCircleImage = point =>
        {
            var pathAndSize = GetCircleImagePathAndSize();
            var circleImage = new NoteImageAndBounds(pathAndSize.Path,
                new Rect(point, pathAndSize.ImageSize));
            return circleImage;
        };
        return GenerateLinesAndNoteImages(getNoteImage, getCircleImage, rythmicGroup);
    }
}