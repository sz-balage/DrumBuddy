using DrumBuddy.Core.Models;

namespace DrumBuddy.Models;

public class SheetOption
{
    public SheetOption(string displayName, Sheet? sheet)
    {
        DisplayName = displayName;
        Sheet = sheet;
    }

    public string DisplayName { get; }
    public Sheet? Sheet { get; }
}