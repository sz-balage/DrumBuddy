using DrumBuddy.Core.Models;

namespace DrumBuddy.IO.Models;

public record CreateSheetRequest(SheetDto Sheet);
public record UpdateSheetRequest(SheetDto Sheet);