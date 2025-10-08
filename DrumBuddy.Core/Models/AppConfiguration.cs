using DrumBuddy.Core.Enums;

namespace DrumBuddy.Core.Models;

public class AppConfiguration
{
    public Dictionary<Drum, int> DrumMapping { get; set; } = new();
    public Dictionary<Drum, int> KeyboardMapping { get; set; } = new();
    public Dictionary<Drum, DrumPositionSlot> DrumPositions { get; set; } = new();
    public int MetronomeVolume { get; set; } = 8000;
    public bool KeyboardInput { get; set; } = false;
    public Dictionary<string, string> UserSettings { get; set; } = new();
}