using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace DrumBuddy.Client.Models;

public class LineAndStroke
{
    public Point StartPoint { get; }
    public Point EndPoint { get; }
    public double StrokeThickness { get; }
    public IBrush StrokeColor { get; } = Brushes.Black;
        
    public LineAndStroke(Point start, Point end, double thickness = 1)
    {
        StartPoint = start;
        EndPoint = end;
        StrokeThickness = thickness;
    }
}