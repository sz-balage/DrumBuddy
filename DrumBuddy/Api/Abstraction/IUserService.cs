using System.Threading.Tasks;

namespace DrumBuddy.Api;

public interface IUserService
{
    string? Email { get; }
    string? UserName { get; }
    bool IsOnline { get; }
    string? UserId { get; set; }
    string? GetToken();
    Task SetToken(string token, string userName, string email, string userId);
    void ClearToken();
    bool IsTokenValid();
    Task SaveRememberedCredentialsAsync(string email, string password);
    Task<(string? Email, string? Password)?> LoadRememberedCredentialsAsync();
    void ClearRememberedCredentials();
}