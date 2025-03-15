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

    public static bool IsOneDrumAwayFrom(this Drum drum, Drum otherDrum)
    {
        return Math.Abs(GetPositionForDrum(drum) - GetPositionForDrum(otherDrum)) == 10;
    }

    //Kick: between line 1 and 2
    //Snare: between line 3 and 4
    //FloorTom: between line 2 and 3
    //Tom1: between line 4 and 5
    //Tom2: on line 4
    //HiHat: above line 5 (between line 5 and the invisible line 6)
    //Ride: on line 5
    //Crash: on line 6
    public static double GetPositionForDrum(Drum drum)
    {
        return drum switch
        {
            Drum.Kick => 65,
            Drum.Snare => 25,
            Drum.FloorTom => 45,
            Drum.Tom1 => 5,
            Drum.Tom2 => 15,
            Drum.Ride => -5,
            Drum.HiHat => -15,
            Drum.Crash1 => -25,
            _ => 35
        };
    }

    public static (Uri Path, Size ImageSize) NoteHeadImagePathAndSize(this Note note)
    {
        return note.Drum switch
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
    }

    public static (Uri Path, Size ImageSize) GetCircleImagePathAndSize()
    {
        return (new Uri(BaseNotationPath + "circle" + ".png"), CircleSize);
    }

    // public static (Uri Path, Size ImageSize) GetSingleRestImagePathAndSize(NoteValue value)
    // {
    //     return value switch
    //     {
    //         NoteValue.Quarter => (new Uri(BaseNotationPath + "quarter_rest" + ImageExtension), QuarterRestImageSize),
    //         NoteValue.Eighth => (new Uri(BaseNotationPath + "eighth_rest" + ImageExtension), EighthRestImageSize),
    //         _ => (new Uri(BaseNotationPath + "sixteenth_rest" + ImageExtension), SixteenthRestImageSize)
    //     };
    // }
}