using System.ComponentModel.DataAnnotations;

namespace DrumBuddy.IO.Models;

public class AuthRequests
{
    public record LoginRequest(
        [Required][EmailAddress] string Email,
        [Required] string Password
    );
    public record RegisterRequest(
        [Required][EmailAddress] string Email,
        [Required][MinLength(6)] string Password,
        string? UserName
    );
}