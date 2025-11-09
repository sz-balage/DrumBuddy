using System.Collections.Generic;
using System.Threading.Tasks;
using DrumBuddy.Api.Models;
using DrumBuddy.Api.Refit;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Models;
using DrumBuddy.Services;

namespace DrumBuddy.Api;

public class ApiClient
{
    private readonly IAuthApi _authApi;
    private readonly ISheetApi _sheetApi;
    private readonly UserService _userService;

    public ApiClient(IAuthApi authApi, ISheetApi sheetApi, UserService userService)
    {
        _authApi = authApi;
        _sheetApi = sheetApi;
        _userService = userService;
    }

    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var request = new AuthRequests.LoginRequest(email, password);
        var result = await _authApi.LoginAsync(request);
         _userService.SetToken(result.Token, result.UserId, result.Email);
        return result;
    }

    public async Task<LoginResponse> RegisterAsync(string email, string password, string? userName = null)
    {
        var request = new AuthRequests.RegisterRequest(email, password, userName);
        var result = await _authApi.RegisterAsync(request);
         _userService.SetToken(result.Token, result.UserId, result.Email);
        return result;
    }
    public async Task<List<Sheet>> GetSheetsAsync() => await _sheetApi.GetSheetsAsync();

    public async Task<Sheet> GetSheetAsync(string name) => await _sheetApi.GetSheetAsync(name);

    public async Task CreateSheetAsync(Sheet sheet) => 
        await _sheetApi.CreateSheetAsync(new CreateSheetRequest(sheet));

    public async Task UpdateSheetAsync(string name, Sheet sheet) => 
        await _sheetApi.UpdateSheetAsync(name, new UpdateSheetRequest(sheet));

    public async Task DeleteSheetAsync(string name) => 
        await _sheetApi.DeleteSheetAsync(name);
}
