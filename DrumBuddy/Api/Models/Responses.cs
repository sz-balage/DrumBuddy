namespace DrumBuddy.Api.Models;

public class LoginResponse
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
public class ForgotPasswordResponse
{
    public string Message { get; set; } = string.Empty;
}