using DrumBuddy.Core.Models;

namespace DrumBuddy.IO.Models;

public record CreateSheetRequest(
    Sheet Sheet
);
public record UpdateSheetRequest(
    Sheet Sheet
);