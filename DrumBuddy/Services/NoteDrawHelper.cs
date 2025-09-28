using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Avalonia;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Extensions;
using DrumBuddy.IO;
using DrumBuddy.Models;
using Splat;
using Note = DrumBuddy.Core.Models.Note;

namespace DrumBuddy.Services;

public class NoteDrawHelper
{
    private const string BaseNotationPath = "avares://DrumBuddy/Assets/Notation/";
    private const string ImageExtension = ".png";

    private const int VerticalLineThickness = 2;
    private const int HorizontalLineThickness = 8;

    private static readonly Size NoteHeadSize = new(24, 20);
    private static readonly Size NoteHeadWithLineSize = new(28, 20);

    private static readonly Size QuarterRestImageSize = new(60, 60);
    private static readonly Size EighthRestImageSize = new(40, 40);
    private static readonly Size SixteenthRestImageSize = new(60, 60);
    private static readonly Size CircleSize = new(8, 8);
    private readonly double _canvasHeight;

    private readonly double _canvasWidth;
    private readonly ConfigurationService _configurationService;
    private readonly double _noteGroupWidth;
    private readonly double _startingXPos;

    public NoteDrawHelper(double canvasWidth, double canvasHeight)
    {
        _configurationService = Locator.Current.GetRequiredService<ConfigurationService>();
        _canvasWidth = canvasWidth;
        _canvasHeight = canvasHeight;
        _startingXPos =
            -1 * (_canvasWidth / 2) +
            20; //since 0 is middle, we need to set it to left by decrementing it by half the width, and then add a minimal offset so it is not on the edge
        _noteGroupWidth = _canvasWidth / 4;
    }

    private double FromCentreBasedToTopLeftCoordinateX(double coordinateX)
    {
        return coordinateX + _canvasWidth / 2;
    }

    private double FromCentreBasedToTopLeftCoordinateY(double coordinateY)
    {
        return coordinateY + _canvasHeight / 2;
    }

    private bool AreOneDrumAway(Drum drum, Drum otherDrum)
    {
        return Math.Abs(GetYPositionForDrum(drum) - GetYPositionForDrum(otherDrum)) == NoteHeadSize.Height / 2;
    }

    private LineAndStroke GetLineForNoteGroup(NoteGroup noteGroup, double highestY, double xPosition)
    {
        //get start and endpoint for each note groups line -> the start point is the y position of the lowest note in the group, the endpoint is the y position of the highest note in the group + 3 * notehead height
        var lowestY = GetYPositionForDrum(noteGroup.First().Drum);
        //var highestY = GetYPositionForDrum(noteGroup.Last().Drum);
        var startPointY = FromCentreBasedToTopLeftCoordinateY(lowestY);
        var endPointY = FromCentreBasedToTopLeftCoordinateY(highestY - 3 * NoteHeadSize.Height);
        return new LineAndStroke(noteGroup, new Point(FromCentreBasedToTopLeftCoordinateX(xPosition) + 12, startPointY),
            new Point(FromCentreBasedToTopLeftCoordinateX(xPosition) + 12, endPointY), VerticalLineThickness);
    }

    /// <summary>
    ///     Get on/or between which line the drum should be drawn.
    /// </summary>
    /// <param name="drum">Drum to be drawn.</param>
    /// <returns></returns>
    private double GetYPositionForDrum(Drum drum)
    {
        return _configurationService.DrumPositions.TryGetValue(drum, out var pos)
            ? pos
            : 30; // fallback
    }

    private static (Uri Path, Size ImageSize) NoteHeadImagePathAndSize(Note note)
    {
        return note.Drum switch
        {
            Drum.HiHat or Drum.HiHat_Open or Drum.HiHat_Pedal or Drum.Ride => (
                new Uri(BaseNotationPath + "note_head_x" + ImageExtension), NoteHeadSize),
            Drum.Crash1 or Drum.Crash2 => (new Uri(BaseNotationPath + "note_head_x_line" + ImageExtension),
                NoteHeadWithLineSize),
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

    /// <summary>
    ///     Generates lines and note images for the given rythmic group.
    /// </summary>
    /// <param name="getCircleImage">Function for creating a circle image.</param>
    /// <param name="rythmicGroup">Containing the note groups to draw.</param>
    /// <param name="getNoteImage">Function for creating a note image</param>
    /// <returns>The lines and images to draw.</returns>
    internal (ImmutableArray<NoteImageAndBounds> Images, ImmutableArray<LineAndStroke> LineAndStrokes)
        GenerateLinesAndNoteImages(
            Func<Note, Point, NoteImageAndBounds> getNoteImage,
            Func<Point, NoteImageAndBounds> getCircleImage,
            RythmicGroup rythmicGroup)
    {
        //TODO: hunt bugs by looking at keyboard input values
        var highestY = rythmicGroup.NoteGroups.Select(ng => ng.Select(n => GetYPositionForDrum(n.Drum)).Min()).Min();
        var linesAndStrokes = new List<LineAndStroke>();
        var images = new List<NoteImageAndBounds>();
        var openHihatIndicators = new List<NoteImageAndBounds>();
        var noteGroups = rythmicGroup.NoteGroups;
        var x = _startingXPos;
        for (var i = 0; i < noteGroups.Length; i++)
        {
            var noteGroup = noteGroups[i];
            noteGroup.Sort((n1, n2) => GetYPositionForDrum(n2.Drum).CompareTo(GetYPositionForDrum(n1.Drum)));
            if (!noteGroup.IsRest)
                linesAndStrokes.Add(GetLineForNoteGroup(noteGroup, highestY, x));
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
                if (noteGroup == noteGroups.Last() && linesAndStrokes.Count > 1)
                {
                    var firstLine = linesAndStrokes.First();
                    var lastLine = linesAndStrokes.Last();
                    var lineAndStroke = new LineAndStroke(noteGroups[i], firstLine.EndPoint, lastLine.EndPoint,
                        HorizontalLineThickness);
                    linesAndStrokes.Add(lineAndStroke);
                }

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
                if (note.Drum == Drum.HiHat_Open)
                {
                    var openHihatIndicator = GetCircleForHiHatOpen(point, note);
                    openHihatIndicators.Add(openHihatIndicator);
                }

                images.Add(noteImage);
            }

            x += GetDisplacementForNoteValue(noteGroup.Value, _noteGroupWidth);
            if (i == noteGroups.Length - 1 && linesAndStrokes.Count > 1)
            {
                var firstLine = linesAndStrokes.First();
                var lastLine = linesAndStrokes.Last();
                var lineAndStroke = new LineAndStroke(noteGroups[i], firstLine.EndPoint, lastLine.EndPoint,
                    HorizontalLineThickness);
                linesAndStrokes.Add(lineAndStroke);
            }
        }

        var sixteenthNotes = noteGroups.Where(ng => ng is { Value: NoteValue.Sixteenth, IsRest: false }).ToList();
        if (sixteenthNotes.Count == 1)
        {
            var sixteenthNoteGroup = sixteenthNotes.Single();
            var lineOfGroup = linesAndStrokes.Single(line =>
                line.NoteGroup == sixteenthNoteGroup && line.LineType == LineType.Vertical);
            var lastNonRest = noteGroups.Last(ng => !ng.IsRest);
            if (lineOfGroup == linesAndStrokes.First() && sixteenthNoteGroup == lastNonRest) //TODO: fix bug
            {
                //draw two lines diagonally
                //TODO: use curly lines instead of straight lines
                var startPoint1 = lineOfGroup.EndPoint;
                var endPoint1 = lineOfGroup.EndPoint.WithX(startPoint1.X + 20).WithY(startPoint1.Y + 40);
                var lineAndStroke1 =
                    new LineAndStroke(sixteenthNoteGroup, startPoint1, endPoint1, VerticalLineThickness);
                var lineAndStroke2 = new LineAndStroke(sixteenthNoteGroup, startPoint1.WithY(startPoint1.Y + 20),
                    endPoint1.WithY(endPoint1.Y + 20), VerticalLineThickness);
                linesAndStrokes.Add(lineAndStroke1);
                linesAndStrokes.Add(lineAndStroke2);
            }
            else
            {
                //it is already connected by one line, so draw a little line below that line
                var isRightSided = noteGroups.First(ng => !ng.IsRest) == sixteenthNoteGroup;

                var startPoint = lineOfGroup.EndPoint.WithX(isRightSided
                    ? lineOfGroup.EndPoint.X
                    : lineOfGroup.EndPoint.X - 10).WithY(lineOfGroup.EndPoint.Y + 10);
                var endPoint = isRightSided
                    ? lineOfGroup.EndPoint.WithX(lineOfGroup.EndPoint.X + 10).WithY(lineOfGroup.EndPoint.Y + 10)
                    : lineOfGroup.EndPoint.WithY(lineOfGroup.EndPoint.Y + 10);
                var lineAndStroke =
                    new LineAndStroke(sixteenthNoteGroup, startPoint, endPoint, HorizontalLineThickness);
                linesAndStrokes.Add(lineAndStroke);
            }
        }
        else if (sixteenthNotes.Count > 1)
        {
            //draw a second line between first sixteenth note and last sixteenth note
            var firstSixteenthNote = sixteenthNotes.First();
            var lastSixteenthNote = sixteenthNotes.Last();
            var firstEndPoint = linesAndStrokes
                .Single(line => line.NoteGroup == firstSixteenthNote && line.LineType == LineType.Vertical).EndPoint;
            var lastEndPoint = linesAndStrokes
                .Single(line => line.NoteGroup == lastSixteenthNote && line.LineType == LineType.Vertical).EndPoint;
            var startPoint = firstEndPoint.WithY(firstEndPoint.Y + 10);
            var endPoint = lastEndPoint.WithY(lastEndPoint.Y + 10);
            var lineAndStroke = new LineAndStroke(noteGroups[0], startPoint, endPoint, HorizontalLineThickness);
            linesAndStrokes.Add(lineAndStroke);
        }

        //draw single line for single eighth note at end of rg
        var noteGroupsWithValue = noteGroups.Where(ng => !ng.IsRest).ToList();
        if (noteGroupsWithValue.Count == 1)
        {
            var noteGroup = noteGroupsWithValue.Single(); //.Value == NoteValue.Eighth
            if (noteGroup.Value == NoteValue.Eighth)
            {
                //TODO: use curly line instead of straight line
                var lineOfNote = linesAndStrokes.Single(line => line.NoteGroup == noteGroup);
                var startPoint = lineOfNote.EndPoint;
                var endPoint = lineOfNote.EndPoint.WithX(startPoint.X + 20).WithY(startPoint.Y + 40);
                var lineAndStroke = new LineAndStroke(noteGroups[0], startPoint, endPoint, VerticalLineThickness);
                linesAndStrokes.Add(lineAndStroke);
            }
        }

        images.AddRange(openHihatIndicators);
        return ([..images], [..linesAndStrokes]);
    }

    private NoteImageAndBounds GetCircleForHiHatOpen(Point point, Note note)
    {
        (Uri Path, Size ImageSize) noteHeadPathAndSize =
            new(new Uri(BaseNotationPath + "circle_open" + ImageExtension), CircleSize);
        var noteImage = new NoteImageAndBounds(noteHeadPathAndSize.Path,
            new Rect(point.WithY(point.Y - 120), noteHeadPathAndSize.ImageSize));
        return noteImage;
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