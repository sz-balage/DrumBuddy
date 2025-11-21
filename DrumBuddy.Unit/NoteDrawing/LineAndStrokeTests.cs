using Avalonia;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Models;
using Shouldly;

namespace DrumBuddy.Unit.NoteDrawing;

public abstract class LineAndStrokeTests
{
    public class WhenLineAndStrokeIsCreated
    {
        [Fact]
        public void WithVerticalPoints_ShouldDetectAsVerticalLine()
        {
            // Arrange
            var noteGroup = new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) });
            var start = new Point(100, 50);
            var end = new Point(100, 150);

            // Act
            var line = new LineAndStroke(noteGroup, start, end);

            // Assert
            line.LineType.ShouldBe(LineType.Vertical);
        }

        [Fact]
        public void WithHorizontalPoints_ShouldDetectAsHorizontalLine()
        {
            // Arrange
            var noteGroup = new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) });
            var start = new Point(50, 100);
            var end = new Point(150, 100);

            // Act
            var line = new LineAndStroke(noteGroup, start, end);

            // Assert
            line.LineType.ShouldBe(LineType.Horizontal);
        }

        [Fact]
        public void WithDiagonalPoints_ShouldDetectAsHorizontalLine()
        {
            // Arrange
            var noteGroup = new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) });
            var start = new Point(50, 50);
            var end = new Point(150, 150);

            // Act
            var line = new LineAndStroke(noteGroup, start, end);

            // Assert
            line.LineType.ShouldBe(LineType.Horizontal);
        }

        [Fact]
        public void WithDefaultThickness_ShouldBeOne()
        {
            // Arrange
            var noteGroup = new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) });
            var start = new Point(100, 50);
            var end = new Point(100, 150);

            // Act
            var line = new LineAndStroke(noteGroup, start, end);

            // Assert
            line.StrokeThickness.ShouldBe(1.0);
        }

        [Fact]
        public void WithCustomThickness_ShouldBeSet()
        {
            // Arrange
            var noteGroup = new NoteGroup(new List<Note> { new(Drum.Kick, NoteValue.Quarter) });
            var start = new Point(100, 50);
            var end = new Point(100, 150);

            // Act
            var line = new LineAndStroke(noteGroup, start, end, 3.5);

            // Assert
            line.StrokeThickness.ShouldBe(3.5);
        }

        [Fact]
        public void ShouldPreserveAllProperties()
        {
            // Arrange
            var noteGroup = new NoteGroup(new List<Note> { new(Drum.Snare, NoteValue.Eighth) });
            var start = new Point(100, 50);
            var end = new Point(100, 150);
            var thickness = 2.0;

            // Act
            var line = new LineAndStroke(noteGroup, start, end, thickness);

            // Assert
            line.NoteGroup.ShouldBe(noteGroup);
            line.StartPoint.ShouldBe(start);
            line.EndPoint.ShouldBe(end);
            line.StrokeThickness.ShouldBe(thickness);
        }
    }
}