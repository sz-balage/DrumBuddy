using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DrumBuddy.Api.Models;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Models;

namespace DrumBuddy.Api;

public interface IApiClient
{
    Task<LoginResponse> LoginAsync(string email, string password);
    Task<ForgotPasswordResponse> ForgotPasswordAsync(string email);
    Task<LoginResponse> RegisterAsync(string email, string password, string? userName = null);
    Task<List<SheetSummaryDto>> GetSheetSummariesAsync();
    Task<Sheet> GetSheetAsync(Guid id);
    Task CreateSheetAsync(Sheet sheet, DateTime syncedAt);
    Task UpdateSheetAsync(Guid id, Sheet sheet, DateTime updatedAt);
    Task DeleteSheetAsync(Guid id);
    Task<ConfigurationResponse> GetConfigurationAsync();
    Task UpdateConfigurationAsync(AppConfiguration configuration, DateTime updatedAt);
}