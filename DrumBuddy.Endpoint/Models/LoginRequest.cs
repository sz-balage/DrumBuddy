using System.ComponentModel.DataAnnotations;

namespace DrumBuddy.Endpoint.Models;

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);