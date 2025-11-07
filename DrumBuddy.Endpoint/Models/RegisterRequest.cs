using System.ComponentModel.DataAnnotations;

namespace DrumBuddy.Endpoint.Models;

public record RegisterRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password,
    string? UserName
);