using System.Collections.Immutable;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Models;

namespace DrumBuddy.IO.Data;

public interface ISheetRepository
{
    Task<ImmutableArray<Sheet>> LoadSheetsAsync(string? userId);
    Task<Sheet?> GetSheetByIdAsync(Guid id, string? userId);
    Task SaveSheetAsync(SheetDto sheetDto, string? userId);
    Task SaveSheetAsync(Sheet sheet, string? userId);
    Task UpdateSheetAsync(SheetDto sheetDto, DateTime updatedAt, string? userId);
    Task UpdateSheetAsync(Sheet sheet, DateTime updatedAt, string? userId);
    Task DeleteSheetAsync(Guid id, string? userId);
    Task CreateUserIfNotExistsAsync(string? userId);
    bool SheetExists(string sheetName, string? userId);
}