using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DrumBuddy.Api.Models;
using DrumBuddy.Api.Refit;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Models;

namespace DrumBuddy.Api;

public class ApiClient
{
    private readonly IAuthApi _authApi;
    private readonly ISheetApi _sheetApi;
    private readonly IConfigurationApi _configurationApi;
    private readonly UserService _userService;

    public ApiClient(
        IAuthApi authApi, 
        ISheetApi sheetApi,
        IConfigurationApi configurationApi,
        UserService userService)
    {
        _authApi = authApi;
        _sheetApi = sheetApi;
        _configurationApi = configurationApi;
        _userService = userService;
    }

    // Auth methods
    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var request = new AuthRequests.LoginRequest(email, password);
        var result = await _authApi.LoginAsync(request);
        _userService.SetToken(result.Token, result.UserName, result.Email);
        return result;
    }
    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(string email)
    {
        var request = new AuthRequests.ForgotPasswordRequest() { Email = email };
        var result = await _authApi.ForgotPasswordAsync(request);
        return result;
    }

    public async Task<LoginResponse> RegisterAsync(string email, string password, string? userName = null)
    {
        var request = new AuthRequests.RegisterRequest(email, password, userName);
        var result = await _authApi.RegisterAsync(request);
        _userService.SetToken(result.Token, result.UserName, result.Email);
        return result;
    }

    // Sheet methods
    public async Task<List<Sheet>> GetSheetsAsync() 
        => await _sheetApi.GetSheetsAsync();

    public async Task<Sheet> GetSheetAsync(Guid id) 
        => await _sheetApi.GetSheetAsync(id);

    public async Task CreateSheetAsync(Sheet sheet) 
        => await _sheetApi.CreateSheetAsync(new CreateSheetRequest(sheet));

    public async Task UpdateSheetAsync(Guid id, Sheet sheet) 
        => await _sheetApi.UpdateSheetAsync(id, new UpdateSheetRequest(sheet));

    public async Task DeleteSheetAsync(Guid id) 
        => await _sheetApi.DeleteSheetAsync(id);

    // Configuration methods
    public async Task<AppConfiguration> GetConfigurationAsync()
        => await _configurationApi.GetConfigurationAsync();

    public async Task UpdateConfigurationAsync(AppConfiguration configuration)
        => await _configurationApi.UpdateConfigurationAsync(
            new UpdateConfigurationRequest(configuration));
}
