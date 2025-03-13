using System;
using Avalonia;
using DrumBuddy.Core.Enums;
using DrumBuddy.IO.Enums;

namespace DrumBuddy.Services;

public static class DrawHelper
{
    private static readonly Size NoteHeadSize = new(24, 20);

    private static readonly Size QuarterRestImageSize = new(60, 60);
    private static readonly Size EighthRestImageSize = new(50, 50);
    private static readonly Size SixteenthRestImageSize = new(40, 40);


    public const double SectionWidth = 31.25; //125/4
    public const double NoteRadius = 10.0;
    public const double RestLineHeight = 20.0;

    private const string BaseNotationPath = "avares://DrumBuddy/Assets/Notation/";
    private const string ImageExtension = ".png";

    public static bool IsOneDrumAwayFrom(this Drum drum, Drum otherDrum)
    {
        return Math.Abs(GetPositionForDrum(drum) - GetPositionForDrum(otherDrum)) == 10;
    }

    //Bass: between line 1 and 2
    //Snare: between line 3 and 4
    //FloorTom: between line 2 and 3
    //Tom1: between line 4 and 5
    //Tom2: on line 4
    //HiHat: on top of line 5 (between line 5 and the invisible line 6)
    public static int GetPositionForDrum(Drum drum)
    {
        return drum switch
        {
            Drum.Bass => 80,
            Drum.Snare => 40,
            Drum.FloorTom => 60,
            Drum.Tom1 => 20,
            Drum.Tom2 => 30,
            Drum.HiHat => 0,
            _ => 0
        };
    }

    public static (Uri Path, Size ImageSize) NoteHeadImagePathAndSize(this Drum drum)
    {
        return drum switch
        {
            Drum.HiHat or Drum.Crash1 or Drum.Crash2 => (new Uri(BaseNotationPath + "note_head_x" + ImageExtension),
                NoteHeadSize),
            Drum.Rest => throw new ArgumentException(
                "Drum must be a note to use this extension method. For rests use the SingleRestPath extension method.",
                nameof(drum)),
            _ => (new Uri(BaseNotationPath + "note_head" + ImageExtension), NoteHeadSize)
        };
    }

    public static (Uri Path, Size ImageSize) GetSingleRestImagePathAndSize(NoteValue value)
    {
        return value switch
        {
            NoteValue.Quarter => (new Uri(BaseNotationPath + "quarter_rest" + ImageExtension), QuarterRestImageSize),
            NoteValue.Eighth => (new Uri(BaseNotationPath + "eighth_rest" + ImageExtension), EighthRestImageSize),
            _ => (new Uri(BaseNotationPath + "sixteenth_rest" + ImageExtension), SixteenthRestImageSize)
        };
    }
}