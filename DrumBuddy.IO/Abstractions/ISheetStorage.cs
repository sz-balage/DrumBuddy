using System.Collections.Immutable;
using DrumBuddy.Core.Models;

namespace DrumBuddy.IO.Abstractions;

public interface ISheetStorage
{
    Task RemoveSheetAsync(Sheet sheet);
    Task SaveSheetAsync(Sheet sheet);
    Task<ImmutableArray<Sheet>> LoadSheetsAsync();
    Task<IEnumerable<string>> GetSavedSheetNames();
    Task RenameFileAsync(string oldFileName, Sheet newSheet);
    bool SheetExists(string sheetName);
}