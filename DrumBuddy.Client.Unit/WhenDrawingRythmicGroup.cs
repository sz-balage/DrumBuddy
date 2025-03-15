using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media.Imaging;
using DrumBuddy.Client.Models;
using DrumBuddy.Client.Services;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Enums;
using static DrumBuddy.Client.ViewModels.HelperViewModels.RythmicGroupViewModel;

namespace DrumBuddy.Client.Unit;

public class WhenDrawingRythmicGroup
{
    private const int NoteGroupWidth = 1;
    private const int StartingXPosition = 1;
    private static Note _hihatNote = new Note(Drum.HiHat, NoteValue.Sixteenth);
    private static Note _kickNote = new Note(Drum.Kick, NoteValue.Sixteenth);
    private static Note _tom1Note = new Note(Drum.Tom1, NoteValue.Sixteenth);
    private static Note _tom2Note = new Note(Drum.Tom2, NoteValue.Sixteenth);
    private static Note _sixteenthRest = new Note(Drum.Rest, NoteValue.Sixteenth);
    private static Note _eighthRest = new Note(Drum.Rest, NoteValue.Eighth);
    private static Note _quarterRest = new Note(Drum.Rest, NoteValue.Eighth);
    [Fact]
    public void ShouldGenerateForSingleRest()
    {
        //Arrange
        var creationData = _quarterRest.NoteHeadImagePathAndSize();
        Bitmap testBitmap = null;
        var expectedResult = (new List<Line>(), new List<NoteImageAndBounds> { new NoteImageAndBounds(testBitmap, new Rect(StartingXPosition, 0, NoteGroupWidth, NoteGroupWidth)) });
        List<NoteGroup> noteGroup =
        [
            new([_quarterRest])
        ];
        var rgToDraw = new RythmicGroup([..noteGroup]);
        Func<Note, Point, NoteImageAndBounds> getNoteImage = (note, point) =>
        {
            return new NoteImageAndBounds(testBitmap, new Rect(point, new Size(NoteGroupWidth, NoteGroupWidth)));
        };
        Func<Point, NoteImageAndBounds> getCircleImage = point =>
        {
            return new NoteImageAndBounds(testBitmap, new Rect(point, new Size(NoteGroupWidth, NoteGroupWidth)));
        };
        // Act
        var result = GenerateLinesAndNoteImages(getNoteImage, getCircleImage, rgToDraw, NoteGroupWidth, StartingXPosition);
        ;
    }
}