using DrumBuddy.Core.Enums;

namespace DrumBuddy.Core.Models;

public class AppConfiguration
{
    public Dictionary<Drum, int> DrumMapping { get; set; } = new();
    public Dictionary<Drum, double> DrumPositions { get; set; } = new();
    public int MetronomeVolume { get; set; } = 8000;
    public bool KeyboardInput { get; set; } = false;
}
