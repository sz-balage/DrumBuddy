using System;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Api.Models;

public class LoginResponse
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
public class ForgotPasswordResponse
{
    public string Message { get; set; } = string.Empty;
}

public class ConfigurationResponse
{
    public AppConfiguration Configuration { get; set; } = new AppConfiguration();
    public DateTime UpdatedAt { get; set; }
}