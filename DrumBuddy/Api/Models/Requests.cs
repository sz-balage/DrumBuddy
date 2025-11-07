using DrumBuddy.Core.Models;

namespace DrumBuddy.Api.Models;

public record RegisterRequest(string Email, string Password, string? UserName);
public record LoginRequest(string Email, string Password);
public record CreateSheetRequest(Sheet Content);
public record UpdateSheetRequest(Sheet Content);