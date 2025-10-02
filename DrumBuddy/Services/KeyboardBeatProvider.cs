using Avalonia.Input;
using DrumBuddy.Core.Enums;

namespace DrumBuddy.Services;

public static class KeyboardBeatProvider
{
    public static int GetDrumValueForKey(Key key)
    {
        return key switch
        {
            Key.A => (int)Drum.HiHat,
            Key.S => (int)Drum.Snare,
            Key.D => (int)Drum.Kick,
            Key.F => (int)Drum.FloorTom,
            Key.Q => (int)Drum.Crash1,
            Key.C => (int)Drum.Crash2,
            Key.W => (int)Drum.Tom1,
            Key.E => (int)Drum.Tom2,
            Key.R => (int)Drum.Ride,
            Key.Y => (int)Drum.HiHat_Open,
            Key.X => (int)Drum.HiHat_Pedal,
            _ => -2
        };
    }

    public static string GetKeyForDrumValue(int value) =>
        value switch
        {
            (int)Drum.HiHat => Key.A.ToString(),
            (int)Drum.Snare => Key.S.ToString(),
            (int)Drum.Kick => Key.D.ToString(),
            (int)Drum.FloorTom => Key.F.ToString(),
            (int)Drum.Crash1 => Key.Q.ToString(),
            (int)Drum.Crash2 => Key.C.ToString(),
            (int)Drum.Tom1 => Key.W.ToString(),
            (int)Drum.Tom2 => Key.E.ToString(),
            (int)Drum.Ride => Key.R.ToString(),
            (int)Drum.HiHat_Open => Key.Y.ToString(),
            (int)Drum.HiHat_Pedal => Key.X.ToString(),
            _ => Key.None.ToString()
        };
}