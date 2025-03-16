using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Client.Models;

public class LineAndStroke
{
    public NoteGroup NoteGroup { get; }
    public Point StartPoint { get; }
    public Point EndPoint { get; }
    public double StrokeThickness { get; }
    public IBrush StrokeColor { get; } = Brushes.Black;
    public LineType LineType { get; }
    
    public LineAndStroke(NoteGroup noteGroup,Point start, Point end, double thickness = 1)
    {
        NoteGroup = noteGroup;
        StartPoint = start;
        EndPoint = end;
        StrokeThickness = thickness;
        LineType = start.X == end.X ? LineType.Vertical : LineType.Horizontal;
    }
}
public enum LineType
{
    Vertical,
    Horizontal
}
