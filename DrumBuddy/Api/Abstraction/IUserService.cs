using System.Threading.Tasks;

namespace DrumBuddy.Api;

public interface IUserService
{
    string? Email { get; }
    string? UserName { get; }
    bool IsOnline { get; }
    string? UserId { get; set; }
    string? GetToken();
    Task SetToken(string token, string refreshToken, string userName, string email, string userId);
    void ClearToken();
    bool IsTokenValid();
    Task SaveRememberedCredentialsAsync(string email, string refreshToken);
    Task<(string? Email, string? RefreshToken)?> LoadRememberedCredentialsAsync();
    void ClearRememberedCredentials();
}