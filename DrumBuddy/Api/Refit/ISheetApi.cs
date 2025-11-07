using System.Collections.Generic;
using System.Threading.Tasks;
using DrumBuddy.Api.Models;
using DrumBuddy.Core.Models;
using Refit;

namespace DrumBuddy.Api.Refit;

public interface ISheetApi
{
    [Get("/api/sheets")]
    Task<List<Sheet>> GetSheetsAsync();

    [Get("/api/sheets/{name}")]
    Task<Sheet> GetSheetAsync(string name);

    [Post("/api/sheets")]
    Task CreateSheetAsync([Body] CreateSheetRequest request);

    [Put("/api/sheets/{name}")]
    Task UpdateSheetAsync(string name, [Body] UpdateSheetRequest request);

    [Delete("/api/sheets/{name}")]
    Task DeleteSheetAsync(string name);
}