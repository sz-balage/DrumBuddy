using System.Threading.Tasks;
using DrumBuddy.Api.Models;
using DrumBuddy.IO.Models;
using Refit;

namespace DrumBuddy.Api.Refit;

public interface IAuthApi
{
    [Post("/api/auth/refresh")]
    Task<RefreshResponse> RefreshAsync([Body] AuthRequests.RefreshRequest request);

    [Post("/api/auth/register")]
    Task<LoginResponse> RegisterAsync([Body] AuthRequests.RegisterRequest request);

    [Post("/api/auth/login")]
    Task<LoginResponse> LoginAsync([Body] AuthRequests.LoginRequest request);

    [Post("/api/auth/forgot-password")]
    Task<ForgotPasswordResponse> ForgotPasswordAsync([Body] AuthRequests.ForgotPasswordRequest request);
}