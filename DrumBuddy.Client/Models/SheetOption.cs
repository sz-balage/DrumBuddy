using DrumBuddy.Core.Models;

namespace DrumBuddy.Client.Models;

public class SheetOption
{
    public string DisplayName { get; }
    public Sheet? Sheet { get; }

    public SheetOption(string displayName, Sheet? sheet)
    {
        DisplayName = displayName;
        Sheet = sheet;
    }
}