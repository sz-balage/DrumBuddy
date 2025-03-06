using DrumBuddy.IO.Enums;

namespace DrumBuddy.Helpers.Services;

public static class DrawHelper
{
    //Bass: between line 1 and 2
    //Snare: between line 3 and 4
    //FloorTom: between line 2 and 3
    //Tom1: between line 4 and 5
    //Tom2: on line 4
    //HiHat: on top of line 5 (between line 5 and the invisible line 6)
    public static double GetPoisitionForBeat(Beat beat) => beat switch
    {
        Beat.Bass => 80,
        Beat.Snare => 40,
        Beat.FloorTom => 60,
        Beat.Tom1 => 20,
        Beat.Tom2 => 30,
        Beat.HiHat => 0,
        _ => 0
    };
}