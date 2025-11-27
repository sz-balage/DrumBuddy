using System.ComponentModel.DataAnnotations;

namespace DrumBuddy.IO.Models;

public class AuthRequests
{
    public record LoginRequest(
        [Required] [EmailAddress] string Email,
        [Required] string Password
    );

    public record RegisterRequest(
        [Required] [EmailAddress] string Email,
        [Required] [MinLength(6)] string Password,
        string? UserName
    );

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public record RefreshRequest(string RefreshToken);

    public record RefreshResponse(
        string UserName,
        string Email,
        string AccessToken,
        string RefreshToken,
        string UserId);
}