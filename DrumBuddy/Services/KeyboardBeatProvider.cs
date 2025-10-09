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
            (int)Drum.HiHat => nameof(Key.A),
            (int)Drum.Snare => nameof(Key.S),
            (int)Drum.Kick => nameof(Key.D),
            (int)Drum.FloorTom => nameof(Key.F),
            (int)Drum.Crash1 => nameof(Key.Q),
            (int)Drum.Crash2 => nameof(Key.C),
            (int)Drum.Tom1 => nameof(Key.W),
            (int)Drum.Tom2 => nameof(Key.E),
            (int)Drum.Ride => nameof(Key.R),
            (int)Drum.HiHat_Open => nameof(Key.Y),
            (int)Drum.HiHat_Pedal => nameof(Key.X),
            _ => nameof(Key.None)
        };
}