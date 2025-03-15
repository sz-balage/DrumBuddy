using System;
using Avalonia;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Enums;

namespace DrumBuddy.Client.Services;

public static class DrawHelper
{
    private const string BaseNotationPath = "avares://DrumBuddy.Client/Assets/Notation/";
    private const string ImageExtension = ".png";
    public static readonly Size NoteHeadSize = new(24, 20);
    private static readonly Size NoteHeadWithLineSize = new(28, 20);

    private static readonly Size QuarterRestImageSize = new(60, 60);
    private static readonly Size EighthRestImageSize = new(40, 40);
    private static readonly Size SixteenthRestImageSize = new(60, 60);
    private static readonly Size CircleSize = new(8, 8);

    public static bool IsOneDrumAwayFrom(this Drum drum, Drum otherDrum) => Math.Abs(GetYPositionForDrum(drum) - GetYPositionForDrum(otherDrum)) == (NoteHeadSize.Height / 2);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="drum"></param>
    /// <returns></returns>
    public static double GetYPositionForDrum(Drum drum) =>
        drum switch
        {
            Drum.Kick => 65, // between line 1 and 2
            Drum.Snare => 25, // between line 3 and 4
            Drum.FloorTom => 45, // between line 2 and 3
            Drum.Tom1 => 5,    // between line 4 and 5
            Drum.Tom2 => 15,    // on line 4
            Drum.Ride => -5,    // on line 5
            Drum.HiHat => -15,    // above line 5 (between line 5 and the invisible line 6)
            Drum.Crash1 => -25,    // on line 6
            _ => 35
        };

    public static (Uri Path, Size ImageSize) NoteHeadImagePathAndSize(this Note note) =>
        note.Drum switch
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

    public static (Uri Path, Size ImageSize) GetCircleImagePathAndSize() => (new Uri(BaseNotationPath + "circle" + ".png"), CircleSize);
}