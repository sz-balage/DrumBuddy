using DrumBuddy.Core.Models;

namespace DrumBuddy.Endpoint.Models;

public record UpdateSheetRequest(
    Sheet Content
);