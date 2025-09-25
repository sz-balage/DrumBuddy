using Avalonia.Input;
using DrumBuddy.Core.Enums;

namespace DrumBuddy.Services;

public static class KeyboardBeatProvider
{
    public static int GetDrumValueForKey(Key key) => key switch
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
        Key.O => (int)Drum.HiHat_Open,
        Key.X => (int)Drum.HiHat_Pedal,
        _ => -2
    };
}