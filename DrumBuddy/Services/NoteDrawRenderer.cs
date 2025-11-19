using System;
using System.Collections.Immutable;
using Avalonia;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Models;
using DrumBuddy.Services.Layout;

namespace DrumBuddy.Services;

public class NoteDrawRenderer
{
    private const string BaseNotationPath = "avares://DrumBuddy/Assets/Notation/";
    private const string ImageExtension = ".png";

    private static readonly Size CircleSize = new(8, 8);
    private static readonly Size NoteHeadSize = new(24, 20);
    private static readonly Size NoteHeadWithLineSize = new(28, 20);

    public (ImmutableArray<NoteImageAndBounds>, ImmutableArray<LineAndStroke>)
        Render(LayoutResult layout)
    {
        var images = ImmutableArray.CreateBuilder<NoteImageAndBounds>();
        var lines = ImmutableArray.CreateBuilder<LineAndStroke>();

        foreach (var ln in layout.Lines)
            lines.Add(new LineAndStroke(
                ln.NoteGroup,
                new Point(ln.Start.X, ln.Start.Y),
                new Point(ln.End.X, ln.End.Y),
                ln.Thickness
            ));

        foreach (var n in layout.Notes)
        {
            var (uri, size) = PathForNote(n.Note);
            images.Add(new NoteImageAndBounds(
                uri,
                new Rect(new Point(n.X, n.Y), size)
            ));
        }

        foreach (var c in layout.Circles)
        {
            var path = new Uri(BaseNotationPath + "circle" + ImageExtension);
            images.Add(new NoteImageAndBounds(
                path,
                new Rect(new Point(c.X, c.Y), CircleSize)
            ));
        }

        return (images.ToImmutable(), lines.ToImmutable());
    }

    private static (Uri, Size) PathForNote(Note note)
    {
        return note.Drum switch
        {
            Drum.HiHat or Drum.HiHat_Open or Drum.HiHat_Pedal or Drum.Ride =>
                (new Uri(BaseNotationPath + "note_head_x" + ImageExtension), NoteHeadSize),

            Drum.Crash1 or Drum.Crash2 =>
                (new Uri(BaseNotationPath + "note_head_x_line" + ImageExtension), NoteHeadWithLineSize),

            Drum.Rest => note.Value switch
            {
                NoteValue.Quarter => (new Uri(BaseNotationPath + "quarter_rest.png"), new Size(60, 60)),
                NoteValue.Eighth => (new Uri(BaseNotationPath + "eighth_rest.png"), new Size(40, 40)),
                _ => (new Uri(BaseNotationPath + "sixteenth_rest.png"), new Size(60, 60))
            },

            _ => (new Uri(BaseNotationPath + "note_head.png"), NoteHeadSize)
        };
    }
}