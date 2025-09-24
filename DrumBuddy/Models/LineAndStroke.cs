using Avalonia;
using Avalonia.Media;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Models;

public class LineAndStroke
{
    public LineAndStroke(NoteGroup noteGroup, Point start, Point end, double thickness = 1)
    {
        NoteGroup = noteGroup;
        StartPoint = start;
        EndPoint = end;
        StrokeThickness = thickness;
        LineType = start.X == end.X ? LineType.Vertical : LineType.Horizontal;
    }

    public NoteGroup NoteGroup { get; }
    public Point StartPoint { get; }
    public Point EndPoint { get; }
    public double StrokeThickness { get; }
    public IBrush StrokeColor { get; } = Brushes.Black;
    public LineType LineType { get; }
}

public enum LineType
{
    Vertical,
    Horizontal
}