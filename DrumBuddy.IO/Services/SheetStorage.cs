using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using DrumBuddy.Core.Abstractions;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Abstractions;

namespace DrumBuddy.IO.Services;

public class SheetStorage : ISheetStorage
{
    private readonly ISerializationService _serializationService;
    private readonly string _saveDirectory;
    private const string FileExtension = ".dby";

    public SheetStorage(ISerializationService serializationService)
    {
        _serializationService = serializationService;

        _saveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "DrumBuddy", "SavedFiles");

        if (!Directory.Exists(_saveDirectory))
            Directory.CreateDirectory(_saveDirectory);
    }

    public async Task SaveSheetAsync(Sheet sheet)
    {
        var normalizedName = NormalizeFileName(sheet.Name);
        var filePath = GetFullPath(normalizedName);
        var json = _serializationService.SerializeSheet(sheet);
        
        await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
    }

    public async Task RemoveSheetAsync(Sheet sheet)
    {
        var fileName = NormalizeFileName(sheet.Name);
        var filePath = GetFullPath(fileName);
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Sheet file not found: {fileName}");
        await Task.Run(() => File.Delete(filePath));    
    }
    public async Task<ImmutableArray<Sheet>> LoadSheetsAsync()
    {
        var filePaths = Directory.EnumerateFiles(_saveDirectory,$"*{FileExtension}");
        var arrayBuilder = ImmutableArray.CreateBuilder<Sheet>(filePaths.Count());
        foreach (var filePath in Directory.EnumerateFiles(_saveDirectory,$"*{FileExtension}"))
        {
            var json = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var sheet = _serializationService.DeserializeSheet(json); //TODO: cannot deserialize BPM property
            arrayBuilder.Add(sheet);
        }
        return arrayBuilder.MoveToImmutable();
    }

    public async Task<IEnumerable<string>> GetSavedSheetNames()
    {
        return (await Task.Run(() =>
            Directory.GetFiles(_saveDirectory, $"*{FileExtension}")
                .Select(p => DenormalizeFileName(Path.GetFileNameWithoutExtension(p)))))!;
    }
    /// <summary>
    /// Renames file from old sheet name to new sheet name.
    /// </summary>
    /// <param name="oldSheetName"></param>
    /// <param name="newSheetName"></param>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    public async Task RenameFileAsync(string oldSheetName, string newSheetName)
    {
        var oldFileName = NormalizeFileName(oldSheetName);
        var newFileName = NormalizeFileName(newSheetName);
        
        var oldPath = GetFullPath(oldFileName);
        var newPath = GetFullPath(NormalizeFileName(newFileName));

        if (!File.Exists(oldPath))
            throw new FileNotFoundException($"Sheet file not found: {oldFileName}");

        if (File.Exists(newPath))
            throw new IOException($"A file with name {newFileName} already exists.");

        await Task.Run(() => File.Move(oldPath, newPath));
    }

    private string NormalizeFileName(string fileName)
    {
        var spacesReplaced = fileName.Replace(' ', '_');

        // Replace invalid filename characters with underscores
        var invalidChars = new string(Path.GetInvalidFileNameChars());
        var invalidCharsPattern = $"[{Regex.Escape(invalidChars)}]";
        var normalized = Regex.Replace(spacesReplaced, invalidCharsPattern, "_");
        return normalized;
    }

    private string DenormalizeFileName(string fileName) => fileName.Replace('_', ' ');

    private string GetFullPath(string fileName) => Path.Combine(_saveDirectory, fileName + FileExtension);
}