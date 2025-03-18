using DrumBuddy.Core.Models;

namespace DrumBuddy.IO.Abstractions;

public interface ISheetStorage
{
    Task SaveSheetAsync(Sheet sheet, string fileName);
    Task<Sheet> LoadSheetAsync(string fileName);
    Task<IEnumerable<string>> GetSavedSheetNames();
    Task RenameFileAsync(string oldFileName, string newFileName);
}