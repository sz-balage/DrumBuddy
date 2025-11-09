using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DrumBuddy.Api.Models;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Models;
using Refit;

namespace DrumBuddy.Api.Refit;

public interface ISheetApi
{
    [Get("/api/sheets")]
    Task<List<Sheet>> GetSheetsAsync();

    [Get("/api/sheets/{id}")]
    Task<Sheet> GetSheetAsync(Guid id);

    [Post("/api/sheets")]
    Task CreateSheetAsync([Body] CreateSheetRequest request);

    [Put("/api/sheets/{id}")]
    Task UpdateSheetAsync(Guid id, [Body] UpdateSheetRequest request);

    [Delete("/api/sheets/{id}")]
    Task DeleteSheetAsync(Guid id);
}