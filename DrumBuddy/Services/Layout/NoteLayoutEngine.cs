using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Models;

namespace DrumBuddy.Services.Layout;

public record LayoutNote(
    Note Note,
    double X,
    double Y,
    bool HasOpenHiHatCircle
);

public record LayoutCircle(
    double X,
    double Y
);

public record LayoutLine(
    NoteGroup NoteGroup,
    (double X, double Y) Start,
    (double X, double Y) End,
    LineType LineType,
    int Thickness
);

public record LayoutResult(
    ImmutableArray<LayoutNote> Notes,
    ImmutableArray<LayoutCircle> Circles,
    ImmutableArray<LayoutLine> Lines
);

public class NoteLayoutEngine
{
    private static readonly (double W, double H) NoteHeadSize = (24, 20);
    private readonly double _canvasHeight;
    private readonly double _canvasWidth;
    private readonly IReadOnlyDictionary<Drum, DrumPositionSlot> _drumPositions;
    private readonly double _noteGroupWidth;
    private readonly double _startingXPos;

    public NoteLayoutEngine(
        double canvasWidth,
        double canvasHeight,
        IReadOnlyDictionary<Drum, DrumPositionSlot> drumPositions)
    {
        _canvasWidth = canvasWidth;
        _canvasHeight = canvasHeight;
        _drumPositions = drumPositions;

        _startingXPos = -(_canvasWidth / 2) + 20;
        _noteGroupWidth = _canvasWidth / 4;
    }

    private double FromCenterX(double x)
    {
        return x + _canvasWidth / 2;
    }

    private double FromCenterY(double y)
    {
        return y + _canvasHeight / 2;
    }

    private double GetYPositionForDrum(Drum drum)
    {
        return _drumPositions.TryGetValue(drum, out var slot)
            ? (int)slot
            : 30;
    }

    private bool AreOneDrumAway(Drum a, Drum b)
    {
        return Math.Abs(GetYPositionForDrum(a) - GetYPositionForDrum(b)) ==
               NoteHeadSize.H / 2;
    }

    private double Displacement(NoteValue val)
    {
        return val switch
        {
            NoteValue.Quarter => _noteGroupWidth * 4,
            NoteValue.Eighth => _noteGroupWidth * 2,
            _ => _noteGroupWidth
        };
    }

    public LayoutResult Generate(RythmicGroup group)
    {
        var notes = new List<LayoutNote>();
        var circles = new List<LayoutCircle>();
        var lines = new List<LayoutLine>();

        if (group.NoteGroups.Length == 0)
            return new LayoutResult([], [], []);

        var noteGroups = group.NoteGroups;

        var highestY = noteGroups
            .Select(ng => ng.Select(n => GetYPositionForDrum(n.Drum)).Min())
            .Min();

        var x = _startingXPos;

        for (var i = 0; i < noteGroups.Length; i++)
        {
            var noteGroup = noteGroups[i];
            noteGroup.Sort((a, b) =>
                GetYPositionForDrum(b.Drum).CompareTo(GetYPositionForDrum(a.Drum)));

            if (!noteGroup.IsRest)
            {
                var lowestY = GetYPositionForDrum(noteGroup.First().Drum);
                var startY = FromCenterY(lowestY);
                var endY = FromCenterY(highestY - 3 * NoteHeadSize.H);

                lines.Add(new LayoutLine(
                    noteGroup,
                    (FromCenterX(x) + 12, startY),
                    (FromCenterX(x) + 12, endY),
                    LineType.Vertical,
                    2
                ));
            }

            //SIXTEENTH REST SPECIAL CASE
            if (noteGroup is { IsRest: true, Value: NoteValue.Sixteenth } && i != 0)
            {
                var prevLayout = notes.Last();
                circles.Add(new LayoutCircle(prevLayout.X + 20, prevLayout.Y));

                x += Displacement(noteGroup.Value);
                AddEndHorizontalIfNeeded(i);
                continue;
            }

            for (var j = 0; j < noteGroup.Count; j++)
            {
                var note = noteGroup[j];
                var y = GetYPositionForDrum(note.Drum);
                var px = x;

                if (j == 1 && AreOneDrumAway(note.Drum, noteGroup[0].Drum))
                    px += NoteHeadSize.W;

                var hasOpen = note.Drum == Drum.HiHat_Open;

                notes.Add(new LayoutNote(
                    note,
                    px,
                    y,
                    hasOpen
                ));

                if (hasOpen)
                    circles.Add(new LayoutCircle(px, y - 20));
            }

            x += Displacement(noteGroup.Value);
            AddEndHorizontalIfNeeded(i);
        }

        HandleSixteenthBeams();
        HandleSingleEighth();

        void AddEndHorizontalIfNeeded(int i)
        {
            if (i == noteGroups.Length - 1 && lines.Count > 1)
            {
                var firstLine = lines.First(l => l.LineType == LineType.Vertical);
                var lastLine = lines.Last(l => l.LineType == LineType.Vertical);

                lines.Add(new LayoutLine(
                    noteGroups[i],
                    firstLine.End,
                    lastLine.End,
                    LineType.Horizontal,
                    8
                ));
            }
        }

        void HandleSixteenthBeams()
        {
            var sixteenths = noteGroups
                .Where(ng => ng.Value == NoteValue.Sixteenth && !ng.IsRest)
                .ToList();

            if (sixteenths.Count == 1)
            {
                var g = sixteenths.Single();
                var line = lines.Single(l => l.NoteGroup == g && l.LineType == LineType.Vertical);

                var lastNonRest = noteGroups.Last(ng => !ng.IsRest);
                if (line == lines.First() && g == lastNonRest)
                {
                    var p = line.End;

                    lines.Add(new LayoutLine(g, p, (p.X + 20, p.Y + 40), LineType.Vertical, 2));
                    lines.Add(new LayoutLine(g, (p.X, p.Y + 20), (p.X + 20, p.Y + 60), LineType.Vertical, 2));
                }
                else
                {
                    var right = noteGroups.First(ng => !ng.IsRest) == g;
                    var start = right ? (line.End.X, line.End.Y + 10) : (line.End.X - 10, line.End.Y + 10);
                    var end = right ? (line.End.X + 10, line.End.Y + 10) : (line.End.X, line.End.Y + 10);

                    lines.Add(new LayoutLine(g, start, end, LineType.Horizontal, 8));
                }
            }
            else if (sixteenths.Count > 1)
            {
                var first = sixteenths.First();
                var last = sixteenths.Last();

                var firstEnd = lines.Single(l => l.NoteGroup == first && l.LineType == LineType.Vertical).End;
                var lastEnd = lines.Single(l => l.NoteGroup == last && l.LineType == LineType.Vertical).End;

                lines.Add(new LayoutLine(
                    noteGroups[0],
                    (firstEnd.X, firstEnd.Y + 10),
                    (lastEnd.X, lastEnd.Y + 10),
                    LineType.Horizontal,
                    8
                ));
            }
        }

        void HandleSingleEighth()
        {
            var nonRest = noteGroups.Where(n => !n.IsRest).ToList();
            if (nonRest.Count == 1 && nonRest[0].Value == NoteValue.Eighth)
            {
                var g = nonRest[0];
                var line = lines.Single(l => l.NoteGroup == g);

                var p = line.End;
                lines.Add(new LayoutLine(
                    noteGroups[0],
                    p,
                    (p.X + 20, p.Y + 40),
                    LineType.Vertical,
                    2
                ));
            }
        }

        return new LayoutResult(
            notes.ToImmutableArray(),
            circles.ToImmutableArray(),
            lines.ToImmutableArray()
        );
    }
}