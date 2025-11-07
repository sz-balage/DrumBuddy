using System.Collections.Generic;
using System.Threading.Tasks;
using DrumBuddy.Api.Models;
using DrumBuddy.Api.Refit;
using DrumBuddy.Core.Models;
using DrumBuddy.Services;

namespace DrumBuddy.Api;

public class ApiClient
{
    private readonly IAuthApi _authApi;
    private readonly ISheetApi _sheetApi;
    private readonly TokenService _tokenService;

    public ApiClient(IAuthApi authApi, ISheetApi sheetApi, TokenService tokenService)
    {
        _authApi = authApi;
        _sheetApi = sheetApi;
        _tokenService = tokenService;
    }

    // Auth methods
    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var request = new LoginRequest(email, password);
        var result = await _authApi.LoginAsync(request);
        await _tokenService.SetTokenAsync(result.Token, result.UserId, result.Email);
        return result;
    }

    public async Task<LoginResponse> RegisterAsync(string email, string password, string? userName = null)
    {
        var request = new RegisterRequest(email, password, userName);
        var result = await _authApi.RegisterAsync(request);
        await _tokenService.SetTokenAsync(result.Token, result.UserId, result.Email);
        return result;
    }

    public async Task LogoutAsync()
    {
        await _tokenService.ClearTokenAsync();
    }

    // Sheet methods
    public async Task<List<Sheet>> GetSheetsAsync() => await _sheetApi.GetSheetsAsync();

    public async Task<Sheet> GetSheetAsync(string name) => await _sheetApi.GetSheetAsync(name);

    public async Task CreateSheetAsync(Sheet sheet) => 
        await _sheetApi.CreateSheetAsync(new CreateSheetRequest(sheet));

    public async Task UpdateSheetAsync(string name, Sheet sheet) => 
        await _sheetApi.UpdateSheetAsync(name, new UpdateSheetRequest(sheet));

    public async Task DeleteSheetAsync(string name) => 
        await _sheetApi.DeleteSheetAsync(name);
}
