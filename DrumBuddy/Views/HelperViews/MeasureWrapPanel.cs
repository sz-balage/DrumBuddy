using System;
using Avalonia;
using Avalonia.Controls;

namespace DrumBuddy.Views.HelperViews;

public class MeasureWrapPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        var measuresPerRow = availableSize.Width >= 1200 ? 2 : 1;
        var childWidth = measuresPerRow > 0 && !double.IsInfinity(availableSize.Width)
            ? availableSize.Width / measuresPerRow
            : 400;
        var childHeight = childWidth / 6; 

        foreach (var child in Children) child.Measure(new Size(childWidth, childHeight));

        var rows = (int)Math.Ceiling((double)Children.Count / measuresPerRow);
        var totalHeight = rows * childHeight;
        var totalWidth = double.IsInfinity(availableSize.Width)
            ? measuresPerRow * childWidth
            : availableSize.Width;

        return new Size(totalWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var measuresPerRow = finalSize.Width >= 1200 ? 2 : 1;
        var childWidth = finalSize.Width / measuresPerRow;
        var childHeight = childWidth / 6;

        double x = 0, y = 0;
        var col = 0;

        foreach (var child in Children)
        {
            child.Arrange(new Rect(x, y, childWidth, childHeight));

            col++;
            if (col >= measuresPerRow)
            {
                col = 0;
                x = 0;
                y += childHeight;
            }
            else
            {
                x += childWidth;
            }
        }

        return finalSize;
    }
}