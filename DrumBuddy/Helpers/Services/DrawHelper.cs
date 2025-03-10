using System;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using DrumBuddy.IO.Enums;

namespace DrumBuddy.Helpers.Services;


public static class DrawHelper
{
    public static bool IsOneDrumAwayFrom(this Drum drum, Drum otherDrum) 
        => Math.Abs(GetPositionForDrum(drum) - GetPositionForDrum(otherDrum)) == 10;
    //Bass: between line 1 and 2
    //Snare: between line 3 and 4
    //FloorTom: between line 2 and 3
    //Tom1: between line 4 and 5
    //Tom2: on line 4
    //HiHat: on top of line 5 (between line 5 and the invisible line 6)
    public static int GetPositionForDrum(Drum drum) => drum switch
    {
        Drum.Bass => 80,
        Drum.Snare => 40,
        Drum.FloorTom => 60,
        Drum.Tom1 => 20,
        Drum.Tom2 => 30,
        Drum.HiHat => 0,
        _ => 0
    };
    public static Image GetImageForBeat(Drum drum) => drum switch
    {
        Drum.Bass => new Image { Source = new Bitmap("Assets/note_head.png") },
        Drum.Snare => new Image { Source = new Bitmap("Assets/note_head.png") },
        Drum.FloorTom => new Image { Source = new Bitmap("Assets/note_head.png") },
        Drum.Tom1 => new Image { Source = new Bitmap("Assets/note_head.png") },
        Drum.Tom2 => new Image { Source = new Bitmap("Assets/note_head.png") },
        Drum.HiHat => new Image { Source = new Bitmap("Assets/note_head_x.png") },
        _ => new Image { Source = new Bitmap("Assets/Rest.png") }
    };
}