using System.Threading.Tasks;
using DrumBuddy.Api.Models;
using Refit;

namespace DrumBuddy.Api.Refit;

public interface IAuthApi
{
    [Post("/api/auth/register")]
    Task<LoginResponse> RegisterAsync([Body] RegisterRequest request);

    [Post("/api/auth/login")]
    Task<LoginResponse> LoginAsync([Body] LoginRequest request);
}